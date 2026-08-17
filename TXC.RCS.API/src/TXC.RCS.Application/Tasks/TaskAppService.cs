using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TXC.RCS.Swagger;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;
using TXC.RCS.Tasks.Mes;
using TXC.RCS.Tasks.OptionCode;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace TXC.RCS.Tasks;

[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
public class TaskAppService : RCSAppService, ITaskAppService
{
    private readonly TaskCreationManager _creator;
    private readonly IOptionCodeSchemaStore _optionSchemas;
    private readonly IRepository<TaskDo, string> _tasks;
    private readonly IRepository<TaskInteractionLog, Guid> _logs;
    private readonly ITaskInteractionLogger _logger;
    private readonly IMesJobResultReporter _mesReporter;
    private readonly IWorkflowTemplateResolver _templates;

    public TaskAppService(
        TaskCreationManager creator,
        IOptionCodeSchemaStore optionSchemas,
        IRepository<TaskDo, string> tasks,
        IRepository<TaskInteractionLog, Guid> logs,
        ITaskInteractionLogger logger,
        IMesJobResultReporter mesReporter,
        IWorkflowTemplateResolver templates)
    {
        _creator = creator;
        _optionSchemas = optionSchemas;
        _tasks = tasks;
        _logs = logs;
        _logger = logger;
        _mesReporter = mesReporter;
        _templates = templates;
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<TaskDto>> GetListAsync(GetTaskListInput input)
    {
        var query = await _tasks.GetQueryableAsync();

        TaskSource? sourceFilter = null;
        if (!string.IsNullOrWhiteSpace(input.Source)
            && Enum.TryParse<TaskSource>(input.Source, true, out var parsedSource))
        {
            sourceFilter = parsedSource;
        }

        TaskLifecycleStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(input.LifecycleStatus)
            && Enum.TryParse<TaskLifecycleStatus>(input.LifecycleStatus, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.Id.Contains(input.Keyword!)
                     || (x.ContainerId != null && x.ContainerId.Contains(input.Keyword!))
                     || (x.LotId != null && x.LotId.Contains(input.Keyword!)))
            .WhereIf(sourceFilter.HasValue, x => x.Source == sourceFilter!.Value)
            .WhereIf(statusFilter.HasValue, x => x.TaskLifecycleStatus == statusFilter!.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.FromAddress),
                x => x.FromAddress == input.FromAddress)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ToAddress),
                x => x.ToAddress == input.ToAddress)
            .WhereIf(!string.IsNullOrWhiteSpace(input.ContainerId),
                x => x.ContainerId == input.ContainerId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.LotId),
                x => x.LotId == input.LotId);

        var total = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(TaskDo.CreationTime) + " desc"
            : input.Sorting;
        query = query.OrderBy(sorting).PageBy(input);
        var items = await AsyncExecuter.ToListAsync(query);
        return new PagedResultDto<TaskDto>(total, items.Select(Map).ToList());
    }

    [AllowAnonymous]
    public async Task<TaskDto> GetAsync(string id)
    {
        var task = await _tasks.GetAsync(id);
        return Map(task);
    }

    [AllowAnonymous]
    public async Task<TaskMonitorDetailDto> GetMonitorDetailAsync(string id)
    {
        var task = await _tasks.GetAsync(id);
        var logs = await _logs.GetListAsync(x => x.TaskId == id);
        logs = logs.OrderBy(x => x.CreationTime).ToList();

        var timeline = await BuildTimelineAsync(task, logs);

        return new TaskMonitorDetailDto
        {
            Task = Map(task),
            Timeline = timeline,
            Logs = logs.Select(MapLog).ToList()
        };
    }

    [AllowAnonymous]
    public async Task<TaskDto> CreateManualAsync(CreateManualTaskDto input)
    {
        Check.NotNullOrWhiteSpace(input.FromAddress, nameof(input.FromAddress));
        Check.NotNullOrWhiteSpace(input.ToAddress, nameof(input.ToAddress));

        var args = new CreateTaskArgs
        {
            FromAddress = input.FromAddress.Trim(),
            FromPort = NullIfWhiteSpace(input.FromPort),
            ToAddress = input.ToAddress.Trim(),
            ToPort = NullIfWhiteSpace(input.ToPort),
            ContainerId = NullIfWhiteSpace(input.ContainerId),
            OptionFields = input.OptionFields
        };

        var task = await _creator.CreateAndStartAsync(args, id: null, source: TaskSource.Manual);
        await _logger.AppendAsync(
            task.Id,
            TaskLogCategories.Operator,
            "Created",
            success: true,
            message: $"{task.FromAddress}/{task.FromPort} → {task.ToAddress}/{task.ToPort}");
        return Map(task);
    }

    [AllowAnonymous]
    public async Task<TaskDto> CancelAsync(CancelTaskDto input)
    {
        Check.NotNullOrWhiteSpace(input.Id, nameof(input.Id));
        var task = await _tasks.GetAsync(input.Id.Trim());
        var reason = NullIfWhiteSpace(input.Reason);
        task.MarkCanceled(reason);
        await _logger.AppendAsync(
            task.Id,
            TaskLogCategories.Operator,
            "Canceled",
            success: true,
            message: reason ?? "已取消");
        await _tasks.UpdateAsync(task, autoSave: true);
        return Map(task);
    }

    [AllowAnonymous]
    public async Task DeleteAsync(string id)
    {
        Check.NotNullOrWhiteSpace(id, nameof(id));
        var task = await _tasks.GetAsync(id.Trim());
        if (task.TaskLifecycleStatus is TaskLifecycleStatus.Pending or TaskLifecycleStatus.Running)
        {
            throw new BusinessException("RCS:TaskDeleteNotAllowed")
                .WithData("TaskId", task.Id)
                .WithData("Status", task.TaskLifecycleStatus);
        }

        await _tasks.DeleteAsync(task, autoSave: true);
    }

    [AllowAnonymous]
    public async Task<MesReportResultDto> RetryMesReportAsync(string id)
    {
        Check.NotNullOrWhiteSpace(id, nameof(id));
        var task = await _tasks.GetAsync(id.Trim());

        if (task.Source != TaskSource.Mes)
        {
            throw new BusinessException("RCS:MesReportNotApplicable")
                .WithData("TaskId", task.Id)
                .WithData("Source", task.Source);
        }

        var jobResult = task.TaskLifecycleStatus switch
        {
            TaskLifecycleStatus.Succeeded => MesJobResults.Completed,
            TaskLifecycleStatus.Canceled => MesJobResults.Deleted,
            _ => throw new BusinessException("RCS:MesReportNotEnded")
                .WithData("TaskId", task.Id)
                .WithData("Status", task.TaskLifecycleStatus)
        };

        var outcome = await _mesReporter.ReportAsync(new MesJobReportRequest
        {
            JobId = task.Id,
            JobResult = jobResult,
            CancelMessage = task.TaskLifecycleStatus == TaskLifecycleStatus.Canceled
                ? task.LastError
                : null
        });

        await _logger.AppendAsync(
            task.Id,
            TaskLogCategories.Mes,
            "RetryReport",
            outcome.Accepted,
            message: outcome.Message);

        return new MesReportResultDto
        {
            Accepted = outcome.Accepted,
            Message = outcome.Message
        };
    }

    [AllowAnonymous]
    public Task<PublishedOptionCodeSchemaDto> GetOptionCodeSchemaAsync()
    {
        var schema = _optionSchemas.GetPublished();
        return Task.FromResult(OptionCodeSchemaMapper.ToPublishedDto(schema));
    }

    private async Task<List<TaskTimelineStepDto>> BuildTimelineAsync(
        TaskDo task,
        List<TaskInteractionLog> logs)
    {
        var def = await _templates.ResolveAsync(task);
        var steps = new List<TaskTimelineStepDto>();

        // 创建节点
        steps.Add(new TaskTimelineStepDto
        {
            Key = "created",
            Label = "创建",
            EventName = "Created",
            Status = "done",
            Time = task.CreationTime
        });

        for (var i = 0; i < def.Steps.Count; i++)
        {
            var step = def.Steps[i];
            var key = step.Id;
            var label = DescribeStep(step);
            string status;
            DateTime? time = null;

            if (task.TaskLifecycleStatus == TaskLifecycleStatus.Canceled)
            {
                status = i < task.StepIndex ? "done" : "canceled";
            }
            else if (task.TaskLifecycleStatus == TaskLifecycleStatus.Failed)
            {
                status = i < task.StepIndex ? "done" : (i == task.StepIndex ? "error" : "pending");
            }
            else if (task.TaskLifecycleStatus == TaskLifecycleStatus.Succeeded)
            {
                status = "done";
            }
            else if (i < task.StepIndex)
            {
                status = "done";
            }
            else if (i == task.StepIndex)
            {
                status = "current";
            }
            else
            {
                status = "pending";
            }

            if (step.Wait != null)
            {
                var hit = logs.LastOrDefault(l =>
                    l.Success
                    && l.Category == TaskLogCategories.Tm
                    && l.EventName == step.Wait.Event
                    && (step.Wait.Leg == null || l.Leg == step.Wait.Leg));
                if (hit != null)
                {
                    time = hit.CreationTime;
                }
            }

            steps.Add(new TaskTimelineStepDto
            {
                Key = key,
                Label = label,
                EventName = step.Wait?.Event ?? step.Activity,
                Leg = step.Wait?.Leg,
                Status = status,
                Time = time
            });
        }

        if (task.TaskLifecycleStatus is TaskLifecycleStatus.Succeeded
            or TaskLifecycleStatus.Canceled
            or TaskLifecycleStatus.Failed)
        {
            steps.Add(new TaskTimelineStepDto
            {
                Key = "terminal",
                Label = task.TaskLifecycleStatus switch
                {
                    TaskLifecycleStatus.Succeeded => "完成",
                    TaskLifecycleStatus.Canceled => "已取消",
                    _ => "失败"
                },
                Status = task.TaskLifecycleStatus == TaskLifecycleStatus.Succeeded
                    ? "done"
                    : task.TaskLifecycleStatus == TaskLifecycleStatus.Canceled
                        ? "canceled"
                        : "error",
                Time = task.LastModificationTime
            });
        }

        return steps;
    }

    private static string DescribeStep(WorkflowStepDefinition step)
    {
        if (step.Wait != null)
        {
            var leg = step.Wait.Leg == TaskLegs.Fetch ? "取" : step.Wait.Leg == TaskLegs.Put ? "放" : "";
            return step.Wait.Event switch
            {
                TaskEvents.TaskStarted => $"{leg}货开始",
                TaskEvents.Arrived => $"{leg}货到达",
                TaskEvents.PermitRequested => $"{leg}货许可",
                TaskEvents.Finished => $"{leg}货完成",
                _ => $"{leg}{step.Wait.Event}"
            };
        }

        if (step.Activity == WorkflowActivities.TmDispatch)
        {
            return "派发 TM";
        }

        if (step.Activity == WorkflowActivities.ExecutionComplete)
        {
            return "收尾";
        }

        return step.Id;
    }

    private static TaskDto Map(TaskDo task) => new()
    {
        Id = task.Id,
        Source = task.Source.ToString(),
        LotId = task.LotId,
        LifecycleStatus = task.TaskLifecycleStatus.ToString(),
        WaitingEvent = task.WaitingEvent,
        ActiveLeg = task.ActiveLeg,
        StepIndex = task.StepIndex,
        FetchTaskSerial = task.FetchTaskSerial,
        PutTaskSerial = task.PutTaskSerial,
        AgvSerial = task.AgvSerial,
        FromAddress = task.FromAddress,
        FromPort = task.FromPort,
        ToAddress = task.ToAddress,
        ToPort = task.ToPort,
        ContainerId = task.ContainerId,
        FetchOptionCode = task.FetchOptionCode,
        PutOptionCode = task.PutOptionCode,
        OptionCodeSchemaCode = task.OptionCodeSchemaCode,
        OptionCodeSchemaVersion = task.OptionCodeSchemaVersion,
        LastError = task.LastError,
        CreationTime = task.CreationTime,
        LastModificationTime = task.LastModificationTime
    };

    private static TaskInteractionLogDto MapLog(TaskInteractionLog log) => new()
    {
        Id = log.Id,
        TaskId = log.TaskId,
        Category = log.Category,
        EventName = log.EventName,
        Leg = log.Leg,
        Message = log.Message,
        DetailJson = log.DetailJson,
        Success = log.Success,
        CreationTime = log.CreationTime
    };

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
