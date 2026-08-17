using Volo.Abp.Domain.Entities.Auditing;

namespace TXC.RCS.Tasks;

/// <summary>
/// 任务交互日志：创建 / TM 回调 / 取消 / MES 上报等关键节点。
/// </summary>
public class TaskInteractionLog : CreationAuditedEntity<Guid>
{
    public string TaskId { get; private set; } = null!;

    /// <summary>Workflow / Tm / Mes / Operator。</summary>
    public string Category { get; private set; } = null!;

    public string EventName { get; private set; } = null!;

    public string? Leg { get; private set; }

    public string? Message { get; private set; }

    public string? DetailJson { get; private set; }

    public bool Success { get; private set; }

    protected TaskInteractionLog()
    {
    }

    public TaskInteractionLog(
        Guid id,
        string taskId,
        string category,
        string eventName,
        bool success,
        string? leg = null,
        string? message = null,
        string? detailJson = null) : base(id)
    {
        TaskId = Check.NotNullOrWhiteSpace(taskId, nameof(taskId));
        Category = Check.NotNullOrWhiteSpace(category, nameof(category));
        EventName = Check.NotNullOrWhiteSpace(eventName, nameof(eventName));
        Success = success;
        Leg = string.IsNullOrWhiteSpace(leg) ? null : leg.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        DetailJson = detailJson;
    }
}
