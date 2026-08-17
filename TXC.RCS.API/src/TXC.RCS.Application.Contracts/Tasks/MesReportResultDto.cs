namespace TXC.RCS.Tasks;

/// <summary>手动重推 RCS-101 的结果。</summary>
public class MesReportResultDto
{
    public bool Accepted { get; set; }
    public string? Message { get; set; }
}
