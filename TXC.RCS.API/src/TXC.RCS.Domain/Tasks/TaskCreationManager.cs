using System.Threading;
using TXC.RCS.Locations;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.OptionCode;
using TXC.RCS.Tasks.Workflow;
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
    private readonly IOptionCodeSchemaStore _optionSchemas;
    private readonly IOptionCodeAssembler _optionAssembler;
    private readonly IOptionCodeEncoder _optionEncoder;

    public TaskCreationManager(
        ITaskIdGenerator ids,
        IErackGate erack,
        IRepository<AddressMap, Guid> addressMaps,
        IRepository<TaskDo, string> tasks,
        ITaskWorkflow workflow,
        IOptionCodeSchemaStore optionSchemas,
        IOptionCodeAssembler optionAssembler,
        IOptionCodeEncoder optionEncoder)
    {
        _ids = ids;
        _erack = erack;
        _addressMaps = addressMaps;
        _tasks = tasks;
        _workflow = workflow;
        _optionSchemas = optionSchemas;
        _optionAssembler = optionAssembler;
        _optionEncoder = optionEncoder;
    }

    /// <summary>
    /// 创建前校验（不落库、不启工作流）。MES 批量「一个失败全失败」时先对本批全部调用。
    /// </summary>
    public async Task EnsureCanCreateAsync(CreateTaskArgs args, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(args.MiddleAddress))
        {
            throw new BusinessException("RCS:MiddleSplitNotImplemented");
        }

        if (string.IsNullOrWhiteSpace(args.ToAddress))
        {
            throw new BusinessException("RCS:ToAddressRequired");
        }

        await FindEnabledMapAsync(args.FromAddress, ct);
        await FindEnabledMapAsync(args.ToAddress!, ct);

        // 干跑编码：点位缺失 / 必填字段等在真正派 TM 前暴露
        var schema = _optionSchemas.GetPublished();
        var fetchFields = await _optionAssembler.AssembleAsync(
            schema, args, args.FromAddress, args.FromPort, TaskLegs.Fetch, ct);
        var putFields = await _optionAssembler.AssembleAsync(
            schema, args, args.ToAddress!, args.ToPort, TaskLegs.Put, ct);
        _ = _optionEncoder.Encode(schema, fetchFields);
        _ = _optionEncoder.Encode(schema, putFields);
    }

    public async Task<TaskDo> CreateAndStartAsync(
        CreateTaskArgs args,
        string? id = null,
        Guid? orderId = null,
        TaskSource source = TaskSource.Manual,
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
            : id.Trim();

        // 3 建聚合（Source / LotId 一并冻结）
        var task = TaskDo.Create(
            taskId,
            orderId,
            source,
            args,
            WorkflowTemplateCatalog.FetchPutCode,
            WorkflowTemplateCatalog.FetchPutVersion);

        // 4 地址映射 → 冻结 TM
        if (string.IsNullOrWhiteSpace(args.ToAddress))
        {
            throw new BusinessException("RCS:ToAddressRequired");
        }

        var fromMap = await FindEnabledMapAsync(args.FromAddress, ct);
        var toMap = await FindEnabledMapAsync(args.ToAddress, ct);
        task.FreezeTmMapping(fromMap.TmTarget, fromMap.TmStorage, toMap.TmTarget, toMap.TmStorage);

        // 5 按当前厂 Schema 编码并冻结（许可只回放，不再计算）
        // agvSlot=Schema const 0；armSide/equipmentType 来自点位表，调用方可不传
        var schema = _optionSchemas.GetPublished();
        var fetchFields = await _optionAssembler.AssembleAsync(
            schema, args, args.FromAddress, args.FromPort, TaskLegs.Fetch, ct);
        var putFields = await _optionAssembler.AssembleAsync(
            schema, args, args.ToAddress!, args.ToPort, TaskLegs.Put, ct);
        task.FreezeOptionCodes(
            _optionEncoder.Encode(schema, fetchFields),
            _optionEncoder.Encode(schema, putFields));
        task.FreezeOptionSchema(schema.Code, schema.Version);

        // 6 落库
        await _tasks.InsertAsync(task, autoSave: false, cancellationToken: ct);

        // 7 启动工作流
        await _workflow.StartAsync(task, ct);

        // 8 持久化运行时字段（WaitingEvent 等）
        await _tasks.UpdateAsync(task, autoSave: false, cancellationToken: ct);

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
