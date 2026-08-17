using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TXC.RCS.Swagger;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.Mes;
using TXC.RCS.Tasks.OptionCode;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace TXC.RCS.Tasks;

/// <summary>
/// <see cref="ITaskAppService"/> 实现：薄应用层，创建逻辑全部委托 <see cref="TaskCreationManager"/>。
/// </summary>
/// <remarks>
/// Swagger 分组：<see cref="RcsSwaggerDocs.Biz"/>（「TXC RCS 业务」标签页）。
/// 后续同业务域的 AppService 也请打上相同的 <c>ApiExplorerSettings.GroupName</c>。
/// </remarks>
[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
public class TaskAppService : RCSAppService, ITaskAppService
{
    private readonly TaskCreationManager _creator;
    private readonly IOptionCodeSchemaStore _optionSchemas;
    private readonly IRepository<TaskDo, string> _tasks;
    private readonly IMesJobResultReporter _mesReporter;

    public TaskAppService(
        TaskCreationManager creator,
        IOptionCodeSchemaStore optionSchemas,
        IRepository<TaskDo, string> tasks,
        IMesJobResultReporter mesReporter)
    {
        _creator = creator;
        _optionSchemas = optionSchemas;
        _tasks = tasks;
        _mesReporter = mesReporter;
    }

    /// <inheritdoc />
    /// <remarks>
    /// S1 临时 <see cref="AllowAnonymousAttribute"/>，便于 Swagger 无登录联调。
    /// 上线前改为权限码（如 <c>RCS.Tasks.Create</c>）。
    /// </remarks>
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
            // 空串 → null，避免把 "" 当有效货号写入
            ContainerId = NullIfWhiteSpace(input.ContainerId),
            OptionFields = input.OptionFields
        };

        // id: null → 人工任务号；Source=Manual（MES 走独立 Ingress）
        var task = await _creator.CreateAndStartAsync(args, id: null, source: TaskSource.Manual);

        return Map(task);
    }

    /// <inheritdoc />
    [AllowAnonymous]
    public async Task<TaskDto> CancelAsync(CancelTaskDto input)
    {
        Check.NotNullOrWhiteSpace(input.Id, nameof(input.Id));
        var task = await _tasks.GetAsync(input.Id.Trim());
        task.MarkCanceled(NullIfWhiteSpace(input.Reason));
        await _tasks.UpdateAsync(task, autoSave: true);
        return Map(task);
    }

    /// <inheritdoc />
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

        return new MesReportResultDto
        {
            Accepted = outcome.Accepted,
            Message = outcome.Message
        };
    }

    /// <inheritdoc />
    [AllowAnonymous]
    public Task<PublishedOptionCodeSchemaDto> GetOptionCodeSchemaAsync()
    {
        var schema = _optionSchemas.GetPublished();
        return Task.FromResult(OptionCodeSchemaMapper.ToPublishedDto(schema));
    }

    /// <summary>聚合 → 对外 DTO（只映射联调所需字段）。</summary>
    private static TaskDto Map(TaskDo task) => new()
    {
        Id = task.Id,
        Source = task.Source.ToString(),
        LotId = task.LotId,
        LifecycleStatus = task.TaskLifecycleStatus.ToString(),
        WaitingEvent = task.WaitingEvent,
        ActiveLeg = task.ActiveLeg,
        FetchTaskSerial = task.FetchTaskSerial,
        PutTaskSerial = task.PutTaskSerial,
        FromAddress = task.FromAddress,
        ToAddress = task.ToAddress,
        ContainerId = task.ContainerId,
        FetchOptionCode = task.FetchOptionCode,
        PutOptionCode = task.PutOptionCode,
        OptionCodeSchemaCode = task.OptionCodeSchemaCode,
        OptionCodeSchemaVersion = task.OptionCodeSchemaVersion
    };

    /// <summary>空白字符串统一当成「未传」。</summary>
    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
