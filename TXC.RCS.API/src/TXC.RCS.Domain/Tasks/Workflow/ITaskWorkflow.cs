using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace TXC.RCS.Tasks.Workflow
{
    public interface ITaskWorkflow
    {
        Task StartAsync(TaskDo task, CancellationToken ct = default);
        Task<IReadOnlyDictionary<string, string>> SignalAsync(TaskDo task, TaskSignal signal, CancellationToken ct = default);
    }
}