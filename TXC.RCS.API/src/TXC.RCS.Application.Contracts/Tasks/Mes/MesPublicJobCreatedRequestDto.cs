using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TXC.RCS.Tasks.Mes;

/// <summary>RCS-001 Public_Job_Created 请求体（厂商 JSON，不进 Domain）。</summary>
public class MesPublicJobCreatedRequestDto
{
    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("job_list")]
    public List<MesJobItemDto>? JobList { get; set; }
}

public class MesJobItemDto
{
    [JsonPropertyName("job_id")]
    public string? JobId { get; set; }

    [JsonPropertyName("lot_id")]
    public string? LotId { get; set; }

    [JsonPropertyName("carrier_id")]
    public string? CarrierId { get; set; }

    [JsonPropertyName("source_location")]
    public string? SourceLocation { get; set; }

    [JsonPropertyName("source_port")]
    public string? SourcePort { get; set; }

    [JsonPropertyName("target_location")]
    public string? TargetLocation { get; set; }

    [JsonPropertyName("target_port")]
    public string? TargetPort { get; set; }
}
