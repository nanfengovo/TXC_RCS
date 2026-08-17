using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TXC.RCS.Tasks.Mes;

/// <summary>Sim：不调真 MES，只记日志。由 Host 按 Mode 注册为 <see cref="IMesJobResultReporter"/>。</summary>
public class SimulationMesJobResultReporter : IMesJobResultReporter
{
    private readonly ILogger<SimulationMesJobResultReporter> _logger;

    public SimulationMesJobResultReporter(ILogger<SimulationMesJobResultReporter> logger)
    {
        _logger = logger;
    }

    public Task<MesJobReportOutcome> ReportAsync(MesJobReportRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "MES Sim report job_id={JobId} job_result={JobResult} cancel_message={CancelMessage}",
            request.JobId,
            request.JobResult,
            request.CancelMessage ?? "");
        return Task.FromResult(MesJobReportOutcome.Ok("sim"));
    }
}
