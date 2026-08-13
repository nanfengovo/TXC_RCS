using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class ActivityContext
    {
        public required TaskDo Task {get; init;}

        public TaskSignal? Signal {get; init;}

        public Dictionary<string,string> ResponseData {get; init;} = new();
    }

    public interface IWorkflowActivity : ITransientDependency
    {
        string Name {get;}

        Task ExecuteAsync(ActivityContext ctx, CancellationToken ct = default);
    }
}