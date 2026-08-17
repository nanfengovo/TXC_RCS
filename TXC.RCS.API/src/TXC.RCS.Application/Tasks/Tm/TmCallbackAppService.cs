using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;

namespace TXC.RCS.Tasks.Tm;

[RemoteService(IsEnabled = false)]
public class TmCallbackAppService : RCSAppService, ITmCallbackAppService
{
    private readonly IRepository<TaskDo, string> _tasks;
    private readonly ITaskWorkflow _workflow;
    private readonly ITaskInteractionLogger _interactionLogger;

    public TmCallbackAppService(
        IRepository<TaskDo, string> tasks,
        ITaskWorkflow workflow,
        ITaskInteractionLogger interactionLogger)
    {
        _tasks = tasks;
        _workflow = workflow;
        _interactionLogger = interactionLogger;
    }

    public async Task<TmCallbackHttpResponse> HandleAsync(string eventName, TmCallbackRequestDto input)
    {
        try
        {
            var serial = input.TaskSerial?.Trim();
            if (string.IsNullOrWhiteSpace(serial))
            {
                return TmCallbackHttpResponse.Fail("task_serial is required");
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                return TmCallbackHttpResponse.Fail("event_name is required");
            }

            var task = await _tasks.FirstOrDefaultAsync(x =>
                x.FetchTaskSerial == serial || x.PutTaskSerial == serial);

            if (task == null)
            {
                return TmCallbackHttpResponse.Fail($"task not found: {serial}");
            }

            var signal = new TaskSignal
            {
                Event = eventName,
                TaskSerial = serial,
                AgvSerial = string.IsNullOrWhiteSpace(input.AgvSerial) ? null : input.AgvSerial.Trim()
            };

            var result = await _workflow.SignalAsync(task, signal);

            if (!result.Accepted)
            {
                await _interactionLogger.AppendAsync(
                    task.Id,
                    TaskLogCategories.Tm,
                    eventName,
                    success: false,
                    message: $"未匹配: waiting {result.ExpectedEvent}/{result.ExpectedLeg}",
                    detailJson: JsonSerializer.Serialize(new { serial, agv = input.AgvSerial }));
                return TmCallbackHttpResponse.Fail(
                    $"signal not matched: waiting {result.ExpectedEvent}/{result.ExpectedLeg},got {result.ActualEvent}/{result.ActualLeg}");
            }

            await _tasks.UpdateAsync(task, true);
            await _interactionLogger.AppendAsync(
                task.Id,
                TaskLogCategories.Tm,
                eventName,
                success: true,
                leg: task.ResolveLegBySerial(serial),
                message: $"serial={serial}",
                detailJson: JsonSerializer.Serialize(new { serial, agv = input.AgvSerial, data = result.Data }));

            return TmCallbackHttpResponse.Ok(result.Data);
        }
        catch (AbpDbConcurrencyException ex)
        {
            Logger.LogWarning(ex, "TM callback concurrency. Serial={Serial} Event={Event}", input.TaskSerial, eventName);
            return TmCallbackHttpResponse.Fail("concurrency conflict");
        }
        catch (BusinessException ex)
        {
            Logger.LogWarning(ex, "TM callback rejected. Serial={Serial} Event={Event}", input.TaskSerial, eventName);
            return TmCallbackHttpResponse.Fail(ex.Code ?? "business error");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "TM callback failed. Serial={Serial} Event={Event}", input.TaskSerial, eventName);
            return TmCallbackHttpResponse.Fail("internal error");
        }
    }
}
