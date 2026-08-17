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
    private readonly ILogger<MesJobResultReportHandler> _logger;

    public MesJobResultReportHandler(
        IMesJobResultReporter reporter,
        ILogger<MesJobResultReportHandler> logger)
    {
        _reporter = reporter;
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
            // Failed 等：按约定不上报
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
        }
        catch (Exception ex)
        {
            // 绝不抛出：否则会回滚任务终态
            _logger.LogError(
                ex,
                "MES RCS-101 failed job_id={JobId} job_result={JobResult}",
                eventData.TaskId,
                jobResult);
        }
    }
}
