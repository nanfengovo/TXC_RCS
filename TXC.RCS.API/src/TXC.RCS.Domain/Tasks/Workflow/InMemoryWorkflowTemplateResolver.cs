using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.Workflow;

public class InMemoryWorkflowTemplateResolver : IWorkflowTemplateResolver, ITransientDependency
{
    public Task<WorkflowTemplateDefinition> ResolveAsync(TaskDo task, CancellationToken ct = default)
    {
        if (task.TemplateCode == WorkflowTemplateCatalog.FetchPutCode
            && task.TemplateVersion == WorkflowTemplateCatalog.FetchPutVersion)
        {
            return Task.FromResult(WorkflowTemplateCatalog.CreateFetchPut());
        }

        throw new BusinessException("RCS:TemplateNotFound")
            .WithData("Code", task.TemplateCode)
            .WithData("Version", task.TemplateVersion);
    }
}
