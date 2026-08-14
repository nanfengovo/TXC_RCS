using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow.Activities;

/// <summary>
/// TM 请求许可时：把当前腿的 option_code / task_serial 写入 ResponseData，供回调 HTTP 原样返回。
/// </summary>
[ExposeServices(typeof(IWorkflowActivity))]
public class TmReplyPermitActivity : IWorkflowActivity, ITransientDependency
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
