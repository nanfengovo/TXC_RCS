using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TXC.RCS.Tasks.EventConst;
using System.Threading;

namespace TXC.RCS.Tasks.Workflow.Activities
{
    public class ExecutionCompleteActivity : IWorkflowActivity
    {
        public string Name => WorkflowActivities.ExecutionComplete;
        public Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default)
        {
            // 成功由引擎步骤耗尽时 MarkSucceeded；这里留空即可
            return Task.CompletedTask;
        }
    }
}