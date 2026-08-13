using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using System.Threading;

namespace TXC.RCS.Tasks.Workflow
{
    public class WorkflowActivityExecutor : IWorkflowActivityExecutor, ITransientDependency
    {
        private readonly IEnumerable<IWorkflowActivity> _activities;
        public WorkflowActivityExecutor(IEnumerable<IWorkflowActivity> activities)
            => _activities = activities;

        public async Task<IReadOnlyDictionary<string, string>> ExecuteAsync(string name, TaskDo task, TaskSignal? signal, CancellationToken ct)
        {
            var act = _activities.FirstOrDefault(x => x.Name == name)
                ?? throw new BusinessException("RCS:ActivityNotFound").WithData("Name", name);

             var ctx = new ActivityContext { Task = task, Signal = signal };
            await act.ExecuteAsync(ctx, ct);
            return ctx.ResponseData;
        }
    }
}