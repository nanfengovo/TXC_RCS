using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;


namespace TXC.RCS.Tasks.Workflow.Activities
{
    public class FakeTmDispatchActivity:IWorkflowActivity
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