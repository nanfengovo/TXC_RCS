using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TXC.RCS.Swagger;
using TXC.RCS.Tasks.Mes;
using Volo.Abp;

namespace TXC.RCS.Controllers.Mes;

/// <summary>
/// MES 入站显式路由。厂商契约要求稳定路径与响应信封，故不用 ABP 约定控制器。
/// </summary>
[RemoteService]
[Route("api/v1/mes")]
[AllowAnonymous] // 厂内不鉴权（与 TM 回调同策略）；上线可改 API Key / 网段
[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
public class MesJobController : RCSController
{
    private readonly IMesJobIngressAppService _ingress;

    public MesJobController(IMesJobIngressAppService ingress)
    {
        _ingress = ingress;
    }

    /// <summary>
    /// RCS-001 派工任务创建（Public_Job_Created）。
    /// MES → RCS；AGV 库位恒 0、臂侧由点位表解析，请求体无需传这些字段。
    /// </summary>
    [HttpPost("Public_Job_Created")]
    public Task<MesApiResponseDto> PublicJobCreatedAsync([FromBody] MesPublicJobCreatedRequestDto body)
        => _ingress.PublicJobCreatedAsync(body);
}
