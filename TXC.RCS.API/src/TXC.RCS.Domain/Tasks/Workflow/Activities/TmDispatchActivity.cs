using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;
using TXC.RCS.Tasks.TM;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow.Activities;

/// <summary>
/// 工作流第一步：组 TM task_add 并下发，再把 Fetch/Put serial 写回任务。
/// </summary>
[ExposeServices(typeof(IWorkflowActivity))]
public class TmDispatchActivity : IWorkflowActivity, ITransientDependency
{
    private readonly ITmClient _tm;
    private readonly ITmTaskPayloadBuilder _builder;

    public TmDispatchActivity(ITmClient tm, ITmTaskPayloadBuilder builder)
    {
        _tm = tm;
        _builder = builder;
    }

    public string Name => WorkflowActivities.TmDispatch;

    public async Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default)
    {
        var built = _builder.Build(ctx.Task);
        await _tm.TaskAddAsync(built.Request, ct);
        ctx.Task.AssignSerial(TaskLegs.Fetch, built.FetchSerial);
        ctx.Task.AssignSerial(TaskLegs.Put, built.PutSerial);
    }
}
