using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TXC.RCS.Tasks.Enums;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace TXC.RCS.Tasks.Mes;

/// <summary>
/// 订阅任务终态 → RCS-101。不抛异常：上报失败不影响本地 Succeeded/Canceled。
/// </summary>
public class MesJobResultReportHandler :
    ILocalEventHandler<TaskLifecycleEndedEvent>,
    ITransientDependency
{
    private readonly IMesJobResultReporter _reporter;
    private readonly ITaskInteractionLogger _interactionLogger;
    private readonly ILogger<MesJobResultReportHandler> _logger;

    public MesJobResultReportHandler(
        IMesJobResultReporter reporter,
        ITaskInteractionLogger interactionLogger,
        ILogger<MesJobResultReportHandler> logger)
    {
        _reporter = reporter;
        _interactionLogger = interactionLogger;
        _logger = logger;
    }

    public async Task HandleEventAsync(TaskLifecycleEndedEvent eventData)
    {
        if (eventData.Source != TaskSource.Mes)
        {
            return;
        }

        string? jobResult = eventData.Status switch
        {
            TaskLifecycleStatus.Succeeded => MesJobResults.Completed,
            TaskLifecycleStatus.Canceled => MesJobResults.Deleted,
            _ => null
        };

        if (jobResult == null)
        {
            return;
        }

        try
        {
            var outcome = await _reporter.ReportAsync(new MesJobReportRequest
            {
                JobId = eventData.TaskId,
                JobResult = jobResult,
                CancelMessage = eventData.CancelMessage
            });

            if (!outcome.Accepted)
            {
                _logger.LogWarning(
                    "MES RCS-101 rejected job_id={JobId} job_result={JobResult} message={Message}",
                    eventData.TaskId,
                    jobResult,
                    outcome.Message);
            }

            await SafeLogAsync(
                eventData.TaskId,
                outcome.Accepted,
                $"job_result={jobResult}; {outcome.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "MES RCS-101 failed job_id={JobId} job_result={JobResult}",
                eventData.TaskId,
                jobResult);
            await SafeLogAsync(eventData.TaskId, false, ex.Message);
        }
    }

    private async Task SafeLogAsync(string taskId, bool success, string? message)
    {
        try
        {
            await _interactionLogger.AppendAsync(
                taskId,
                TaskLogCategories.Mes,
                "JobResultReport",
                success,
                message: message);
        }
        catch (Exception logEx)
        {
            _logger.LogWarning(logEx, "MES report log append failed job_id={JobId}", taskId);
        }
    }
}
