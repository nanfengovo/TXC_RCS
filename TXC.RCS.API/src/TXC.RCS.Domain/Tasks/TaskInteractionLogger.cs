using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace TXC.RCS.Tasks;

public interface ITaskInteractionLogger
{
    Task AppendAsync(
        string taskId,
        string category,
        string eventName,
        bool success,
        string? leg = null,
        string? message = null,
        string? detailJson = null,
        CancellationToken ct = default);
}

public class TaskInteractionLogger : DomainService, ITaskInteractionLogger, ITransientDependency
{
    private readonly IRepository<TaskInteractionLog, Guid> _logs;
    private readonly IGuidGenerator _guids;

    public TaskInteractionLogger(
        IRepository<TaskInteractionLog, Guid> logs,
        IGuidGenerator guids)
    {
        _logs = logs;
        _guids = guids;
    }

    public async Task AppendAsync(
        string taskId,
        string category,
        string eventName,
        bool success,
        string? leg = null,
        string? message = null,
        string? detailJson = null,
        CancellationToken ct = default)
    {
        await _logs.InsertAsync(
            new TaskInteractionLog(
                _guids.Create(),
                taskId,
                category,
                eventName,
                success,
                leg,
                message,
                detailJson),
            autoSave: true,
            cancellationToken: ct);
    }
}

public static class TaskLogCategories
{
    public const string Workflow = "Workflow";
    public const string Tm = "Tm";
    public const string Mes = "Mes";
    public const string Operator = "Operator";
}
