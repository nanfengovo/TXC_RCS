using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TXC.RCS.Swagger;
using Volo.Abp;

namespace TXC.RCS.Tasks;

/// <summary>
/// <see cref="ITaskAppService"/> 实现：薄应用层，创建逻辑全部委托 <see cref="TaskCreationManager"/>。
/// </summary>
/// <remarks>
/// Swagger 分组：<see cref="RcsSwaggerDocs.Biz"/>（「TXC RCS 业务」标签页）。
/// 后续同业务域的 AppService 也请打上相同的 <c>ApiExplorerSettings.GroupName</c>。
/// </remarks>
[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
public class TaskAppService : RCSAppService, ITaskAppService
{
    private readonly TaskCreationManager _creator;

    public TaskAppService(TaskCreationManager creator)
    {
        _creator = creator;
    }

    /// <inheritdoc />
    /// <remarks>
    /// S1 临时 <see cref="AllowAnonymousAttribute"/>，便于 Swagger 无登录联调。
    /// 上线前改为权限码（如 <c>RCS.Tasks.Create</c>）。
    /// </remarks>
    [AllowAnonymous]
    public async Task<TaskDto> CreateManualAsync(CreateManualTaskDto input)
    {
        Check.NotNullOrWhiteSpace(input.FromAddress, nameof(input.FromAddress));
        Check.NotNullOrWhiteSpace(input.ToAddress, nameof(input.ToAddress));

        var args = new CreateTaskArgs
        {
            FromAddress = input.FromAddress.Trim(),
            FromPort = NullIfWhiteSpace(input.FromPort),
            ToAddress = input.ToAddress.Trim(),
            ToPort = NullIfWhiteSpace(input.ToPort),
            // 空串 → null，避免把 "" 当有效货号写入
            ContainerId = NullIfWhiteSpace(input.ContainerId)
        };

        // id: null → 人工任务号；MES 接入时传 job_id
        var task = await _creator.CreateAndStartAsync(args, id: null);

        return Map(task);
    }

    /// <summary>聚合 → 对外 DTO（只映射联调所需字段）。</summary>
    private static TaskDto Map(TaskDo task) => new()
    {
        Id = task.Id,
        LifecycleStatus = task.TaskLifecycleStatus.ToString(),
        WaitingEvent = task.WaitingEvent,
        ActiveLeg = task.ActiveLeg,
        FetchTaskSerial = task.FetchTaskSerial,
        PutTaskSerial = task.PutTaskSerial,
        FromAddress = task.FromAddress,
        ToAddress = task.ToAddress,
        ContainerId = task.ContainerId
    };

    /// <summary>空白字符串统一当成「未传」。</summary>
    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
