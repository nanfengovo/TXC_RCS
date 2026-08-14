using System.Threading;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;
using TXC.RCS.Tasks.Workflow;

namespace TXC.RCS.Tasks.Workflow.Activities
{
    /// <summary>仅供测试手工 new；生产勿注册为 IWorkflowActivity。</summary>
    public class FakeTmDispatchActivity : IWorkflowActivity
    {
        public string Name => WorkflowActivities.TmDispatch;

        public Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default)
        {
            ctx.Task.AssignSerial(TaskLegs.Fetch, $"{ctx.Task.Id}-FETCH");
            ctx.Task.AssignSerial(TaskLegs.Put, $"{ctx.Task.Id}-PUT");
            return Task.CompletedTask;
        }
    }
}