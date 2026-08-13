using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Workflow
{
    public sealed class WorkflowStepDefinition
    {
        public required string Id { get; init;}

        public string? Activity { get; init;}

        public WorkflowWaitDefinition? Wait { get; init;}
    }

    public sealed class WorkflowTemplateDefinition
    {
        public required string Code { get; init;}

        public int Version { get; init; }

        public List<WorkflowStepDefinition> Steps { get; init; } = new();
    }
}