using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TXC.RCS.Options;
using TXC.RCS.Tasks.Mes;

namespace TXC.RCS.Mes;

/// <summary>Real：HTTP 调用 MES RCS2MES_Job_Result_Report。</summary>
public class HttpMesJobResultReporter : IMesJobResultReporter
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MesOptions _options;
    private readonly ILogger<HttpMesJobResultReporter> _logger;

    public HttpMesJobResultReporter(
        IHttpClientFactory httpClientFactory,
        IOptions<MesOptions> options,
        ILogger<HttpMesJobResultReporter> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MesJobReportOutcome> ReportAsync(MesJobReportRequest request, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("Mes");
        var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
        client.BaseAddress ??= new Uri(baseUrl);

        var body = new MesJobResultReportHttpRequest
        {
            JobId = request.JobId,
            JobResult = request.JobResult,
            CancelMessage = request.CancelMessage ?? ""
        };

        using var response = await client.PostAsJsonAsync(
            _options.JobResultReportPath.TrimStart('/'),
            body,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning(
                "MES HTTP {Status} job_id={JobId} body={Body}",
                (int)response.StatusCode,
                request.JobId,
                text);
            return MesJobReportOutcome.Reject($"HTTP {(int)response.StatusCode}");
        }

        var parsed = await response.Content.ReadFromJsonAsync<MesJobResultReportHttpResponse>(cancellationToken: ct);
        if (parsed == null)
        {
            return MesJobReportOutcome.Reject("empty response");
        }

        // 协议层 Success / job_status 任一拒绝都算未接受
        if (parsed.Success == false)
        {
            return MesJobReportOutcome.Reject(parsed.Message ?? parsed.JobMessage ?? "Success=false");
        }

        if (!string.Equals(parsed.JobStatus, MesJobStatuses.Accepted, StringComparison.Ordinal))
        {
            return MesJobReportOutcome.Reject(parsed.JobMessage ?? $"job_status={parsed.JobStatus}");
        }

        return MesJobReportOutcome.Ok(parsed.JobMessage);
    }

    private sealed class MesJobResultReportHttpRequest
    {
        [JsonPropertyName("job_id")]
        public string JobId { get; set; } = "";

        [JsonPropertyName("job_result")]
        public string JobResult { get; set; } = "";

        [JsonPropertyName("cancel_message")]
        public string CancelMessage { get; set; } = "";
    }

    private sealed class MesJobResultReportHttpResponse
    {
        [JsonPropertyName("job_id")]
        public string? JobId { get; set; }

        [JsonPropertyName("job_status")]
        public string? JobStatus { get; set; }

        [JsonPropertyName("job_message")]
        public string? JobMessage { get; set; }

        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("Success")]
        public bool? Success { get; set; }

        [JsonPropertyName("Message")]
        public string? Message { get; set; }
    }
}
