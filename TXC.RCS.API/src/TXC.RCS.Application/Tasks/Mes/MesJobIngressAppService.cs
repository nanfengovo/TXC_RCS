using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TXC.RCS.Tasks.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace TXC.RCS.Tasks.Mes;

/// <summary>
/// RCS-001 入站应用服务：厂商 JSON → <see cref="CreateTaskArgs"/> → <see cref="TaskCreationManager"/>。
/// <para>
/// 特殊逻辑：同批 job_list 必须全部可接受才成功；先预校验再创建，避免半成功。
/// 幂等键以 job_id（= TaskDo.Id）为准；同号同内容直接成功，同号不同内容整批失败。
/// </para>
/// </summary>
[RemoteService(IsEnabled = false)]
public class MesJobIngressAppService : RCSAppService, IMesJobIngressAppService
{
    private readonly IRepository<TaskDo, string> _tasks;
    private readonly TaskCreationManager _creator;

    public MesJobIngressAppService(
        IRepository<TaskDo, string> tasks,
        TaskCreationManager creator)
    {
        _tasks = tasks;
        _creator = creator;
    }

    public async Task<MesApiResponseDto> PublicJobCreatedAsync(MesPublicJobCreatedRequestDto input)
    {
        var requestId = NullIfWhiteSpace(input.RequestId);
        Logger.LogInformation("MES Public_Job_Created request_id={RequestId}", requestId);

        try
        {
            var jobs = input.JobList ?? [];
            if (jobs.Count == 0)
            {
                return MesApiResponseDto.Fail("400", "job_list 不能为空");
            }

            // 文档：绑定任务最多两条；先硬限制，避免误传大批量
            if (jobs.Count > 2)
            {
                return MesApiResponseDto.Fail("400", "job_list 最多 2 条任务");
            }

            var planned = new List<PlannedJob>(jobs.Count);
            foreach (var item in jobs)
            {
                var mapped = MapOrFail(item);
                if (mapped.Error != null)
                {
                    return MesApiResponseDto.Fail("400", mapped.Error);
                }

                planned.Add(mapped.Job!);
            }

            if (planned.Select(x => x.JobId).Distinct(StringComparer.Ordinal).Count() != planned.Count)
            {
                return MesApiResponseDto.Fail("400", "job_list 内 job_id 重复");
            }

            // —— 幂等 / 冲突：任一冲突则整批失败 ——
            var toCreate = new List<PlannedJob>();
            foreach (var job in planned)
            {
                var existing = await _tasks.FindAsync(job.JobId);
                if (existing == null)
                {
                    toCreate.Add(job);
                    continue;
                }

                if (existing.Source != TaskSource.Mes || !existing.MatchesMesDispatch(job.Args))
                {
                    var reason = existing.Source != TaskSource.Mes
                        ? $"来源为 {existing.Source}，不能被 MES 同号覆盖"
                        : existing.DescribeMesDispatchDiff(job.Args);
                    return MesApiResponseDto.Fail(
                        "409",
                        $"{job.JobId} 任务已存在且内容冲突：{reason}");
                }

                // 同号同内容：跳过创建（幂等成功）
                Logger.LogInformation("MES job_id={JobId} idempotent hit", job.JobId);
            }

            // —— 预校验全部待建（一个失败则全失败，且尚未派 TM）——
            foreach (var job in toCreate)
            {
                await _creator.EnsureCanCreateAsync(job.Args);
            }

            foreach (var job in toCreate)
            {
                await _creator.CreateAndStartAsync(
                    job.Args,
                    id: job.JobId,
                    source: TaskSource.Mes);
            }

            return MesApiResponseDto.Ok();
        }
        catch (BusinessException ex)
        {
            var message = FriendlyMessage(ex);
            Logger.LogWarning(ex, "MES Public_Job_Created rejected. request_id={RequestId}", requestId);
            return MesApiResponseDto.Fail("400", message);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "MES Public_Job_Created failed. request_id={RequestId}", requestId);
            return MesApiResponseDto.Fail("500", "RCS 内部异常");
        }
    }

    private static (PlannedJob? Job, string? Error) MapOrFail(MesJobItemDto item)
    {
        var jobId = NullIfWhiteSpace(item.JobId);
        if (jobId == null)
        {
            return (null, "job_id 不能为空");
        }

        var from = NullIfWhiteSpace(item.SourceLocation);
        var to = NullIfWhiteSpace(item.TargetLocation);
        var lot = NullIfWhiteSpace(item.LotId);
        var carrier = NullIfWhiteSpace(item.CarrierId);

        if (from == null)
        {
            return (null, $"{jobId} 缺少 source_location");
        }

        if (to == null)
        {
            return (null, $"{jobId} 缺少 target_location");
        }

        if (lot == null)
        {
            return (null, $"{jobId} 缺少 lot_id");
        }

        if (carrier == null)
        {
            return (null, $"{jobId} 缺少 carrier_id");
        }

        // 不映射 option_code / 臂侧 / AGV 库位：臂侧来自点位表，AGV 库位 Schema 恒 0
        var args = new CreateTaskArgs
        {
            FromAddress = from,
            FromPort = NullIfWhiteSpace(item.SourcePort),
            ToAddress = to,
            ToPort = NullIfWhiteSpace(item.TargetPort),
            ContainerId = carrier,
            LotId = lot
        };

        return (new PlannedJob(jobId, args), null);
    }

    private string FriendlyMessage(BusinessException ex)
    {
        if (string.IsNullOrWhiteSpace(ex.Code))
        {
            return string.IsNullOrWhiteSpace(ex.Message) ? "业务拒绝" : ex.Message;
        }

        var text = L[ex.Code].Value;
        if (ex.Data != null)
        {
            foreach (DictionaryEntry entry in ex.Data)
            {
                var key = entry.Key?.ToString();
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                text = text.Replace("{" + key + "}", entry.Value?.ToString() ?? "", StringComparison.Ordinal);
            }
        }

        return string.IsNullOrWhiteSpace(text) ? ex.Code : text;
    }

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record PlannedJob(string JobId, CreateTaskArgs Args);
}
