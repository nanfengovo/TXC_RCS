using System;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;


namespace TXC.RCS.Tasks.Tm
{
    [RemoteService(IsEnabled = false)]
    public class TmCallbackAppService : RCSAppService, ITmCallbackAppService
    {
        private readonly IRepository<TaskDo,string>  _tasks;

        private readonly ITaskWorkflow _workflow;

        public TmCallbackAppService(IRepository<TaskDo,string> tasks,ITaskWorkflow workflow)
        {
            _tasks = tasks;
            _workflow = workflow;
        }

        public async Task<TmCallbackHttpResponse> HandleAsync(string eventName,TmCallbackRequestDto input)
        {

            try
            {
                var serial = input.TaskSerial?.Trim();
                if(string.IsNullOrWhiteSpace(serial))
                {
                    return TmCallbackHttpResponse.Fail("task_serial is required");
                }

                if(string.IsNullOrWhiteSpace(eventName))
                {
                    return TmCallbackHttpResponse.Fail("event_name is required");
                }

                var task = await _tasks.FirstOrDefaultAsync(x => x.FetchTaskSerial == serial || x.PutTaskSerial == serial);

                if(task == null)
                {
                    return TmCallbackHttpResponse.Fail($"task not found: {serial}");
                }

                var signal = new TaskSignal
                {
                    Event = eventName,
                    TaskSerial = serial,
                    AgvSerial = string.IsNullOrWhiteSpace(input.AgvSerial)?null : input.AgvSerial.Trim()
                };

                var result = await _workflow.SignalAsync(task,signal);

                if(!result.Accepted)
                {
                    // 没推进就不要update
                    return TmCallbackHttpResponse.Fail($"signal not matched: waiting {result.ExpectedEvent}/{result.ExpectedLeg},got {result.ActualEvent}/{result.ActualLeg}");
                }

                await _tasks.UpdateAsync(task,true);

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
}