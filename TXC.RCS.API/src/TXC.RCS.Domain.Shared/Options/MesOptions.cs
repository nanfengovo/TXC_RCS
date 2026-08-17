namespace TXC.RCS.Options;

/// <summary>MES 出站（RCS-101）配置。入站 RCS-001 不依赖此项。</summary>
public class MesOptions
{
    public const string SectionName = "Mes";

    /// <summary>Sim = 只打日志；Real = HTTP 调 MES。</summary>
    public string Mode { get; set; } = "Sim";

    public string BaseUrl { get; set; } = "http://127.0.0.1:9998";

    /// <summary>文档接口名：RCS2MES_Job_Result_Report。</summary>
    public string JobResultReportPath { get; set; } = "api/v1/mes/RCS2MES_Job_Result_Report";
}
