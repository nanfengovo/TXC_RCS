using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Tasks.Mes;

/// <summary>
/// MES 入站（RCS-001）。由显式 Controller 调用；不暴露 ABP 约定路由。
/// </summary>
public interface IMesJobIngressAppService : IApplicationService
{
    /// <summary>Public_Job_Created：MES 派工创建任务。</summary>
    Task<MesApiResponseDto> PublicJobCreatedAsync(MesPublicJobCreatedRequestDto input);
}
