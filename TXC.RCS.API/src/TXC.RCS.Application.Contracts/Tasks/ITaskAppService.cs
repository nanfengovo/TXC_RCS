using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Tasks;

/// <summary>
/// 搬运任务应用服务。
/// <para>
/// 覆盖人工建单、（后续）查询/取消等；MES 下发可复用同一套 Domain 创建管道，
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
    ///   <item>冻结 option_code（S1 占位 <c>0,0</c>）</item>
    ///   <item>落库 → 启动工作流 → 第一步 <c>Tm.Dispatch</c>（Sim/Real 由配置决定）</item>
    /// </list>
    /// <para><b>成功后典型状态</b>：<c>LifecycleStatus=Running</c>，
    /// <c>WaitingEvent=TaskStarted</c>（等 TM <c>task_info</c>），
    /// 并已写入 <c>FetchTaskSerial</c> / <c>PutTaskSerial</c>。</para>
    /// <para><b>地址码</b>须存在于 <c>_RCS.TXC_AddressMaps</c> 且 Enabled
    /// （种子示例：<c>ERACK</c> / <c>H044</c> / <c>H099</c>）。</para>
    /// </remarks>
    /// <param name="input">起终点与可选货号；Port / ContainerId 可空。</param>
    /// <returns>创建后的任务快照（含 serial，便于 Postman 模拟 TM 回调）。</returns>
    Task<TaskDto> CreateManualAsync(CreateManualTaskDto input);
}
