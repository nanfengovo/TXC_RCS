namespace TXC.RCS.Tasks;

/// <summary>取消任务入参。</summary>
public class CancelTaskDto
{
    /// <summary>任务 Id（MES job_id 或人工号）。</summary>
    public string Id { get; set; } = "";

    /// <summary>取消原因 → RCS-101 cancel_message（仅 Mes 单会上报）。</summary>
    public string? Reason { get; set; }
}
