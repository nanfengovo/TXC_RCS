using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.EventConst;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow.Activities;

/// <summary>
/// Catalog 收尾 Activity；真正 Succeeded 由引擎步骤耗尽时 MarkSucceeded。
/// </summary>
[ExposeServices(typeof(IWorkflowActivity))]
public class ExecutionCompleteActivity : IWorkflowActivity, ITransientDependency
{
    public string Name => WorkflowActivities.ExecutionComplete;

    public Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default)
        => Task.CompletedTask;
}
