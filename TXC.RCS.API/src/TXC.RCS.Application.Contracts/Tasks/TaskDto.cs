namespace TXC.RCS.Tasks;

/// <summary>
/// 任务对外快照（给前端 / Swagger / 联调）。
/// <para>细粒度工作流步进看 Domain；这里只暴露联调常用字段。</para>
/// </summary>
public class TaskDto
{
    /// <summary>任务 Id（人工生成或 MES job_id）。</summary>
    public string Id { get; set; } = "";

    /// <summary>建单来源：Manual / Mes。</summary>
    public string Source { get; set; } = "";

    /// <summary>MES 批次号；人工可空。</summary>
    public string? LotId { get; set; }

    /// <summary>
    /// 生命周期：Pending / Running / Succeeded / Failed / Canceled。
    /// </summary>
    public string LifecycleStatus { get; set; } = "";

    /// <summary>
    /// 当前等待的 TM/业务事件名（如 <c>TaskStarted</c>、<c>Arrived</c>）。
    /// 无等待时为 null。
    /// </summary>
    public string? WaitingEvent { get; set; }

    /// <summary>当前腿：Fetch 或 Put。</summary>
    public string? ActiveLeg { get; set; }

    /// <summary>
    /// 取货子任务 serial（发给 TM，回调按此反查腿）。
    /// 形如 <c>{Id}_GET_{yyyyMMddHHmmssfff}</c>。
    /// </summary>
    public string? FetchTaskSerial { get; set; }

    /// <summary>放货子任务 serial。</summary>
    public string? PutTaskSerial { get; set; }

    /// <summary>起点地址码。</summary>
    public string FromAddress { get; set; } = "";

    /// <summary>终点地址码。</summary>
    public string? ToAddress { get; set; }

    /// <summary>容器 / 货号（可空）。</summary>
    public string? ContainerId { get; set; }

    /// <summary>取货腿冻结的 option_code（形如 <c>257,131074</c>）。</summary>
    public string FetchOptionCode { get; set; } = "";

    /// <summary>放货腿冻结的 option_code。</summary>
    public string PutOptionCode { get; set; } = "";

    /// <summary>编码所用 Schema 代码。</summary>
    public string? OptionCodeSchemaCode { get; set; }

    /// <summary>编码所用 Schema 版本。</summary>
    public int OptionCodeSchemaVersion { get; set; }
}
