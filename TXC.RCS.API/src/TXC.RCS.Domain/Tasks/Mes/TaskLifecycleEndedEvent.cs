using TXC.RCS.Tasks.Enums;

namespace TXC.RCS.Tasks.Mes;

/// <summary>
/// 任务进入终态（完成/取消）时的领域事件。
/// Failed 不上报 MES，故失败路径不发此事件（或 Status≠Succeeded/Canceled 时 Handler 忽略）。
/// </summary>
public class TaskLifecycleEndedEvent
{
    public string TaskId { get; }
    public TaskSource Source { get; }
    public TaskLifecycleStatus Status { get; }
    public string? CancelMessage { get; }

    public TaskLifecycleEndedEvent(
        string taskId,
        TaskSource source,
        TaskLifecycleStatus status,
        string? cancelMessage = null)
    {
        TaskId = taskId;
        Source = source;
        Status = status;
        CancelMessage = cancelMessage;
    }
}
