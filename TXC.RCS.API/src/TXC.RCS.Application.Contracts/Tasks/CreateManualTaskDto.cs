namespace TXC.RCS.Tasks;

/// <summary>
/// 人工建单入参。
/// <para>
/// 只描述「业务地址 / 货号」；TM 站点号由 RCS 查 <c>AddressMap</c> 后冻结到任务上，
/// 调用方不要传 TM target。
/// </para>
/// </summary>
public class CreateManualTaskDto
{
    /// <summary>
    /// 起点地址码（RCS 逻辑地址，须能命中 AddressMap）。
    /// 示例：<c>ERACK</c>、<c>H044</c>。
    /// </summary>
    public string FromAddress { get; set; } = "";

    /// <summary>
    /// 起点 Port / Erack 口位（可空）。
    /// S1 可不传；真 Erack 时对应料口 id，并持久化到任务 <c>FromPort</c>。
    /// </summary>
    public string? FromPort { get; set; }

    /// <summary>
    /// 终点地址码（必填）。示例：<c>H044</c>、<c>H099</c>。
    /// </summary>
    public string ToAddress { get; set; } = "";

    /// <summary>
    /// 终点 Port（可空）。
    /// </summary>
    public string? ToPort { get; set; }

    /// <summary>
    /// 容器 / 货号（可空）。对应 TM <c>cargo_id</c>、Erack <c>item</c>。
    /// 空字符串会被服务端收成 null。
    /// </summary>
    public string? ContainerId { get; set; }
}
