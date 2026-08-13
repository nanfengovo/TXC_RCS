using System.Threading;
using System.Threading.Tasks;

namespace TXC.RCS.Locations;

public interface IErackGate
{
    /// <summary>
    /// 建单前：校验（及 RCS 逻辑占槽）。S1 NoOp 直接成功。
    /// </summary>
    Task EnsureReadyAsync(TaskCreateErackRequest request, CancellationToken ct = default);
}

public sealed class TaskCreateErackRequest
{
    public required string? FromAddress { get; init; }
    public string? FromPort { get; init; }
    public string? ToAddress { get; init; }
    public string? ToPort { get; init; }
    public string? ContainerId { get; init; }
}
