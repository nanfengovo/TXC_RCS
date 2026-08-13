using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace TXC.RCS.Tasks.Workflow
{
    public interface IWorkflowTemplateResolver
    {
        Task<WorkflowTemplateDefinition> ResolveAsync(TaskDo task, CancellationToken ct = default);
    }
}