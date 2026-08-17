using System.Threading;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Mes;

/// <summary>
/// RCS-101 出站端口。HTTP / Sim 实现放 Host 或 Domain，Domain 不引用厂商 JSON。
/// </summary>
public interface IMesJobResultReporter
{
    Task<MesJobReportOutcome> ReportAsync(MesJobReportRequest request, CancellationToken ct = default);
}

public sealed class MesJobReportRequest
{
    public required string JobId { get; init; }

    /// <summary><see cref="MesJobResults"/>。</summary>
    public required string JobResult { get; init; }

    public string? CancelMessage { get; init; }
}

public sealed class MesJobReportOutcome
{
    /// <summary>MES job_status=1 且 HTTP/协议成功。</summary>
    public bool Accepted { get; init; }

    public string? Message { get; init; }

    public static MesJobReportOutcome Ok(string? message = null)
        => new() { Accepted = true, Message = message };

    public static MesJobReportOutcome Reject(string? message)
        => new() { Accepted = false, Message = message };
}
