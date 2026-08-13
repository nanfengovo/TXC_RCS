using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class TaskSignal
    {

        public string Event { get; init; } = default!;
        public string? Leg { get; init; }
        public string? TaskSerial { get; init; }
        public string? AgvSerial { get; init; }
        public IReadOnlyDictionary<string, string>? Data { get; init; }
    }
}