using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.EventConst;

namespace TXC.RCS.Tasks.Workflow.Activities;

public class TmReplyPermitActivity : IWorkflowActivity
{
    public string Name => WorkflowActivities.TmReplyPermit;

    public Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default)
    {
         var leg = ctx.Signal?.Leg ?? ctx.Task.ActiveLeg ?? TaskLegs.Fetch;
        var optionCode = ctx.Task.GetOptionCode(leg);
        ctx.ResponseData["option_code"] = optionCode;
        ctx.ResponseData["task_serial"] = ctx.Signal?.TaskSerial ?? "";
        return Task.CompletedTask;
    }
}