using System.Text.Json.Serialization;

namespace TXC.RCS.Tasks.Mes;

/// <summary>MES 通用响应信封（RCS-001 / 后续 101 复用字段形状）。</summary>
public class MesApiResponseDto
{
    [JsonPropertyName("Code")]
    public string Code { get; set; } = "200";

    [JsonPropertyName("Success")]
    public bool Success { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; } = "";

    /// <summary>文档样例：yyyyMMddHHmmss。</summary>
    [JsonPropertyName("DateTime")]
    public string DateTime { get; set; } = "";

    public static MesApiResponseDto Ok(string? message = null) => new()
    {
        Code = "200",
        Success = true,
        Message = message ?? "",
        DateTime = NowStamp()
    };

    public static MesApiResponseDto Fail(string code, string message) => new()
    {
        Code = string.IsNullOrWhiteSpace(code) ? "400" : code,
        Success = false,
        Message = message ?? "",
        DateTime = NowStamp()
    };

    private static string NowStamp() => System.DateTime.Now.ToString("yyyyMMddHHmmss");
}
