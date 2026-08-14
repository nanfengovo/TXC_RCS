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
    /// 起点设备库位（Fetch 腿 TaskCode 的 equipmentSlot）。须为整数。
    /// Schema 中 <c>source=port</c> 且 required 时必填。
    /// </summary>
    public string? FromPort { get; set; }

    /// <summary>
    /// 终点地址码（必填）。示例：<c>H044</c>、<c>H099</c>。
    /// </summary>
    public string ToAddress { get; set; } = "";

    /// <summary>
    /// 终点设备库位（Put 腿 TaskCode 的 equipmentSlot）。须为整数。
    /// Schema 中 <c>source=port</c> 且 required 时必填。
    /// </summary>
    public string? ToPort { get; set; }

    /// <summary>
    /// 容器 / 货号（可空）。对应 TM <c>cargo_id</c>、Erack <c>item</c>。
    /// 空字符串会被服务端收成 null。
    /// </summary>
    public string? ContainerId { get; set; }

    /// <summary>
    /// TaskCode 人工字段（<c>source=args</c>）。key 对齐 GET option-code-schema 的 Inputs。
    /// 晶技 DEMO：<c>armSide</c>、<c>agvSlot</c>；不要传 <c>equipmentType</c> / <c>pickPlace</c>。
    /// 设备库位走 <see cref="FromPort"/> / <see cref="ToPort"/>，不要放进本字典。
    /// </summary>
    public System.Collections.Generic.Dictionary<string, int>? OptionFields { get; set; }
}
