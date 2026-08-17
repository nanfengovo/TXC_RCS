using System.Threading.Tasks;
using TXC.RCS.Tasks.OptionCode;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Tasks;

/// <summary>
/// 搬运任务应用服务。
/// <para>
/// 覆盖人工建单、（后续）查询/取消等；MES 派工走
/// <c>POST /api/v1/mes/Public_Job_Created</c>，复用同一套 Domain 创建管道，
/// 不必另写第二套状态机。
/// </para>
/// <para>
/// HTTP（ABP 约定控制器）：
/// <c>POST /api/app/task/manual</c> —— 注意方法名 <c>CreateManualAsync</c> 会被剥掉
/// 前缀 <c>Create</c>，路由是 <c>manual</c> 而不是 <c>create-manual</c>。
/// </para>
/// </summary>
public interface ITaskAppService : IApplicationService
{
    /// <summary>
    /// 人工创建并启动一条 Fetch→Put 任务。
    /// </summary>
    /// <remarks>
    /// <para><b>管道</b>（Domain <c>TaskCreationManager</c>）：</para>
    /// <list type="number">
    ///   <item>ErackGate 校验（S1 = NoOp）</item>
    ///   <item>生成任务 Id（人工不传 → <c>ITaskIdGenerator</c>）</item>
    ///   <item>AddressMap 解析并冻结 TM target/storage</item>
    ///   <item>按 Published Schema 编码并冻结 option_code（缺 required 字段则创建失败）</item>
    ///   <item>落库 → 启动工作流 → 第一步 <c>Tm.Dispatch</c>（Sim/Real 由配置决定）</item>
    /// </list>
    /// <para><b>成功后典型状态</b>：<c>LifecycleStatus=Running</c>，
    /// <c>WaitingEvent=TaskStarted</c>（等 TM <c>task_info</c>），
    /// 并已写入 <c>FetchTaskSerial</c> / <c>PutTaskSerial</c>。</para>
    /// <para><b>地址码</b>须存在于 <c>_RCS.TXC_AddressMaps</c> 且 Enabled
    /// （种子示例：<c>ERACK</c> / <c>H044</c> / <c>H099</c>）。</para>
    /// </remarks>
    /// <param name="input">起终点、Port（设备库位）与 optionFields（见 GET option-code-schema 的 Inputs）。</param>
    /// <returns>创建后的任务快照（含 serial 与冻结的 option_code）。</returns>
    Task<TaskDto> CreateManualAsync(CreateManualTaskDto input);

    /// <summary>
    /// 取消任务。Mes 来源会经领域事件触发 RCS-101（job_result=2）。
    /// </summary>
    /// <remarks>HTTP：<c>POST /api/app/task/cancel</c>。</remarks>
    Task<TaskDto> CancelAsync(CancelTaskDto input);

    /// <summary>
    /// 对已结束的 Mes 任务手动重推 RCS-101（上报失败后可重试；不改本地生命周期）。
    /// </summary>
    /// <remarks>HTTP：<c>POST /api/app/task/{id}/retry-mes-report</c>。</remarks>
    Task<MesReportResultDto> RetryMesReportAsync(string id);

    /// <summary>
    /// 当前厂 Published TaskCode Schema（位表 + 人工建单 Inputs 绑定）。
    /// </summary>
    /// <remarks>
    /// HTTP：<c>GET /api/app/task/option-code-schema</c>。
    /// 画位表用 <c>Parts</c>；动态表单只渲染 <c>Inputs</c>（不要把 master/leg 做成输入）。
    /// </remarks>
    Task<PublishedOptionCodeSchemaDto> GetOptionCodeSchemaAsync();
}
