using System.Threading.Tasks;
using TXC.RCS.Tasks.OptionCode;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Tasks;

/// <summary>
/// 搬运任务应用服务（人工建单 / 查询 / 取消 / 监控）。
/// MES 派工：<c>POST /api/v1/mes/Public_Job_Created</c>。
/// </summary>
public interface ITaskAppService : IApplicationService
{
    Task<PagedResultDto<TaskDto>> GetListAsync(GetTaskListInput input);

    Task<TaskDto> GetAsync(string id);

    Task<TaskMonitorDetailDto> GetMonitorDetailAsync(string id);

    Task<TaskDto> CreateManualAsync(CreateManualTaskDto input);

    Task<TaskDto> CancelAsync(CancelTaskDto input);

    /// <summary>软删除已结束任务（Pending/Running 请先取消）。</summary>
    Task DeleteAsync(string id);

    Task<MesReportResultDto> RetryMesReportAsync(string id);

    Task<PublishedOptionCodeSchemaDto> GetOptionCodeSchemaAsync();
}
