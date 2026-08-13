using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class WorkflowWaitDefinition
    {
        public required string Event { get; init; }
        public string? Leg { get; init; }
        public bool Matches(TaskSignal signal)
            => Event == signal.Event
            && (Leg == null || Leg == signal.Leg);
    }
}