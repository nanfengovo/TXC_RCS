using System;
using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Locations;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TXC.RCS.Tasks;

public class TaskCreationManager : DomainService
{
    private readonly ITaskIdGenerator _ids;
    private readonly IErackGate _erack;
    private readonly IRepository<AddressMap, Guid> _addressMaps;
    private readonly IRepository<TaskDo, string> _tasks;
    private readonly ITaskWorkflow _workflow;

    public TaskCreationManager(
        ITaskIdGenerator ids,
        IErackGate erack,
        IRepository<AddressMap, Guid> addressMaps,
        IRepository<TaskDo, string> tasks,
        ITaskWorkflow workflow)
    {
        _ids = ids;
        _erack = erack;
        _addressMaps = addressMaps;
        _tasks = tasks;
        _workflow = workflow;
    }

    public async Task<TaskDo> CreateAndStartAsync(
        CreateTaskArgs args,
        string? id = null,
        Guid? orderId = null,
        CancellationToken ct = default)
    {
        // S1: 有中间点先不支持（以后 Ingress 拆成两条任务）
        if (!string.IsNullOrWhiteSpace(args.MiddleAddress))
        {
            throw new BusinessException("RCS:MiddleSplitNotImplemented");
        }

        // 1 Erack（S1 = NoOp）
        await _erack.EnsureReadyAsync(new TaskCreateErackRequest
        {
            FromAddress = args.FromAddress,
            FromPort = args.FromPort,
            ToAddress = args.ToAddress,
            ToPort = args.ToPort,
            ContainerId = args.ContainerId
        }, ct);

        // 2 Id：人工 null → 生成；MES 传入 job_id
        var taskId = string.IsNullOrWhiteSpace(id)
            ? await _ids.NextAsync(ct)
            : id;

        // 3 建聚合
        var task = TaskDo.Create(
            taskId,
            orderId,
            args,
            WorkflowTemplateCatalog.FetchPutCode,
            WorkflowTemplateCatalog.FetchPutVersion);

        // 4 地址映射 → 冻结 TM
        var fromMap = await FindEnabledMapAsync(args.FromAddress, ct);
        if (string.IsNullOrWhiteSpace(args.ToAddress))
        {
            throw new BusinessException("RCS:ToAddressRequired");
        }

        var toMap = await FindEnabledMapAsync(args.ToAddress, ct);
        var fromStorage = Check.NotNullOrWhiteSpace(fromMap.TmStorage, nameof(fromMap.TmStorage), 64);
        var toStorage = Check.NotNullOrWhiteSpace(toMap.TmStorage, nameof(toMap.TmStorage), 64);
        task.FreezeTmMapping(fromMap.TmTarget, fromStorage, toMap.TmTarget, toStorage);

        // 5 OptionCode 占位
        task.FreezeOptionCodes("0,0", "0,0");

        // 6 落库
        await _tasks.InsertAsync(task, autoSave: true, cancellationToken: ct);

        // 7 启动工作流
        await _workflow.StartAsync(task, ct);

        // 8 持久化运行时字段（WaitingEvent 等）
        await _tasks.UpdateAsync(task, autoSave: true, cancellationToken: ct);

        return task;
    }

    private async Task<AddressMap> FindEnabledMapAsync(string addressCode, CancellationToken ct)
    {
        var map = await _addressMaps.FirstOrDefaultAsync(
            x => x.AddressCode == addressCode && x.IsEnabled,
            cancellationToken: ct);

        if (map == null)
        {
            throw new BusinessException("RCS:AddressMapNotFound")
                .WithData("Address", addressCode);
        }

        return map;
    }
}
