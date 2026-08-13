using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace TXC.RCS.Tasks.Workflow
{
    public interface IWorkflowActivityExecutor
    {
        Task<IReadOnlyDictionary<string, string>> ExecuteAsync(string name, TaskDo task, TaskSignal? signal, CancellationToken ct);
    }
}