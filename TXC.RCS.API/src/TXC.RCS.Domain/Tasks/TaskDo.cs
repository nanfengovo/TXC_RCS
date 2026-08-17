using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace TXC.RCS.Tasks
{
    public class TaskDo : FullAuditedAggregateRoot<string>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        /// <value></value>

        public override string Id { get; protected set; }


        /// <summary>
        /// 外部任务ID
        /// </summary>
        /// <value></value>
        public Guid? OrderId { get; private set; }

        /// <summary>
        /// 建单来源（Manual / Mes）。不要用 OrderId 猜测。
        /// </summary>
        public TaskSource Source { get; private set; }

        /// <summary>
        /// MES 批次号（lot_id）；人工可空。
        /// </summary>
        public string? LotId { get; private set; }

        /// <summary>
        /// 容器ID
        /// </summary>
        /// <value></value>
        public string? ContainerId { get; private set; }

        /// <summary>
        /// 起始地址
        /// </summary>
        /// <value></value>
        public string FromAddress { get; private set; }


        /// <summary>
        /// 起始端口
        /// </summary>
        /// <value></value>
        public string? FromPort { get; private set; }

        /// <summary>
        /// 中间地址
        /// </summary>
        /// <value></value>
        public string? MiddleAddress { get; private set; }

        /// <summary>
        /// 中间端口
        /// </summary>
        /// <value></value>
        public string? MiddlePort { get; private set; }

        /// <summary>
        /// 终点地址
        /// </summary>
        /// <value></value>
        public string? ToAddress { get; private set; }

        /// <summary>
        /// 终点端口
        /// </summary>
        /// <value></value>
        public string? ToPort { get; private set; }

        /// <summary>
        /// 取料数量
        /// </summary>
        /// <value></value>

        public int? FetchCount { get; private set; }

        /// <summary>
        /// 放料数量
        /// </summary>
        /// <value></value>

        public int? PutCount { get; private set; }

        /// <summary>
        /// 取料料盒码
        /// </summary>
        /// <value></value>

        public string? FetchMaterialCode { get; private set; }

        /// <summary>
        /// 放料料盒码
        /// </summary>
        /// <value></value>

        public string? PutMaterialCode { get; private set; }

        /// <summary>
        /// 取料设备码
        /// </summary>
        /// <value></value>

        public string? FetchEquipmentCode { get; private set; }

        /// <summary>
        /// 放料设备码
        /// </summary>
        /// <value></value>

        public string? PutEquipmentCode { get; private set; }

        //----------------- TM标识 ---------------

        /// <summary>
        /// 取料任务序列号
        /// </summary>
        /// <value></value>

        public string? FetchTaskSerial { get; private set; }

        /// <summary>
        /// 放料任务序列号
        /// </summary>
        /// <value></value>

        public string? PutTaskSerial { get; private set; }

        /// <summary>
        /// AGV序列号
        /// </summary>
        /// <value></value>

        public string? AgvSerial { get; private set; }

        //----------------- TM映射快照（创建时冻结）---------------
        /// <summary>
        /// 起始TM目标（TM调度地图里的站点）
        /// </summary>
        /// <value></value>
        public int FromTmTarget { get; private set; }

        /// <summary>
        /// 起始TM存储/port口
        /// </summary>
        /// <value></value>
        public string FromTmStorage { get; private set; } = string.Empty;


        /// <summary>
        /// 终点TM目标（TM调度地图里的站点）
        /// </summary>
        /// <value></value>
        public int ToTmTarget { get; private set; }

        /// <summary>
        /// 终点TM存储/port口
        /// </summary>
        /// <value></value>
        public string ToTmStorage { get; private set; } = string.Empty;

        /// <summary>
        /// 取料任务码
        /// </summary>
        /// <value></value>

        public string FetchOptionCode { get; private set; } = string.Empty;

        /// <summary>
        /// 放料任务码
        /// </summary>
        /// <value></value>

        public string PutOptionCode { get; private set; } = string.Empty;

        public string OptionCodeSchemaCode { get; private set; } = string.Empty;

        public int OptionCodeSchemaVersion { get; private set; }

        //----------------- 生命周期（粗颗粒度，给前端使用）---------------

        /// <summary>
        /// 任务生命周期状态
        /// </summary>
        /// <value></value>
        public TaskLifecycleStatus TaskLifecycleStatus { get; private set; }

        //----------------- 模版运行时（细颗粒度，给后端状态机使用）---------------
        /// <summary>
        /// 模版代码
        /// </summary>
        /// <value></value>
        public string TemplateCode { get; private set; }

        /// <summary>
        /// 模版版本
        /// </summary>
        /// <value></value>
        public int TemplateVersion { get; private set; }

        /// <summary>
        /// 步骤索引 表示当前步骤
        /// </summary>
        /// <value></value>
        public int StepIndex { get; private set; }

        /// <summary>
        /// 等待事件 表示当前等待的事件
        /// </summary>
        /// <value></value>

        public string? WaitingEvent { get; private set; }

        /// <summary>
        /// 活动步骤 表示当前活动的步骤 Fetch/Put
        /// </summary>
        /// <value></value> 

        public string? ActiveLeg { get; private set; }

        /// <summary>
        /// 步骤快照 这份任务当初按哪套步骤跑
        /// </summary>
        /// <value></value>

        public string? StepsSnapshotJson { get; private set; }

        /// <summary> 
        /// 运行时 报文  跑到一半攒下的变量
        /// </summary>
        /// <value></value>
        public string? RuntimeVarsJson { get; private set; }

        /// <summary>
        /// 最后错误
        /// </summary>
        /// <value></value>
        public string? LastError { get; private set; }

        protected TaskDo()
        {
            Id = null!;
            TemplateCode = null!;
        }

        protected TaskDo(
            string id,
            Guid? orderId,
            TaskSource source,
            CreateTaskArgs args,
            string templateCode,
            int templateVersion) : base(id)
        {
            Id = id;
            OrderId = orderId;
            Source = source;
            LotId = args.LotId;
            ContainerId = args.ContainerId;
            FromAddress = args.FromAddress;
            FromPort = args.FromPort;
            MiddleAddress = args.MiddleAddress;
            MiddlePort = args.MiddlePort;
            ToAddress = args.ToAddress;
            ToPort = args.ToPort;
            FetchCount = args.FetchCount;
            PutCount = args.PutCount;
            FetchMaterialCode = args.FetchMaterialCode;
            PutMaterialCode = args.PutMaterialCode;
            FetchEquipmentCode = args.FetchEquipmentCode;
            PutEquipmentCode = args.PutEquipmentCode;

            TemplateCode = templateCode;
            TemplateVersion = templateVersion;
            TaskLifecycleStatus = TaskLifecycleStatus.Pending;
            StepIndex = 0;
        }

        public static TaskDo Create(
            string id,
            Guid? orderId,
            TaskSource source,
            CreateTaskArgs args,
            string templateCode,
            int templateVersion)
        {
            Check.NotNullOrWhiteSpace(id, nameof(id));
            Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode));
            return new TaskDo(id, orderId, source, args, templateCode, templateVersion);
        }

        /// <summary>
        /// MES 幂等：同 job_id 时比较「派工语义」是否一致（不含运行时字段）。
        /// </summary>
        public bool MatchesMesDispatch(CreateTaskArgs args)
        {
            return Same(FromAddress, args.FromAddress)
                   && Same(FromPort, args.FromPort)
                   && Same(ToAddress, args.ToAddress)
                   && Same(ToPort, args.ToPort)
                   && Same(ContainerId, args.ContainerId)
                   && Same(LotId, args.LotId);
        }

        /// <summary>同号不同内容时的差异说明（中文，给 MES Message）。</summary>
        public string DescribeMesDispatchDiff(CreateTaskArgs args)
        {
            var parts = new List<string>();
            if (!Same(FromAddress, args.FromAddress))
            {
                parts.Add($"起点地址现有={FromAddress} 请求={args.FromAddress}");
            }

            if (!Same(FromPort, args.FromPort))
            {
                parts.Add($"起点口现有={FromPort ?? ""} 请求={args.FromPort ?? ""}");
            }

            if (!Same(ToAddress, args.ToAddress))
            {
                parts.Add($"终点地址现有={ToAddress ?? ""} 请求={args.ToAddress ?? ""}");
            }

            if (!Same(ToPort, args.ToPort))
            {
                parts.Add($"终点口现有={ToPort ?? ""} 请求={args.ToPort ?? ""}");
            }

            if (!Same(ContainerId, args.ContainerId))
            {
                parts.Add($"料盒现有={ContainerId ?? ""} 请求={args.ContainerId ?? ""}");
            }

            if (!Same(LotId, args.LotId))
            {
                parts.Add($"批次现有={LotId ?? ""} 请求={args.LotId ?? ""}");
            }

            return parts.Count == 0 ? "内容一致" : string.Join("；", parts);
        }

        private static bool Same(string? a, string? b)
            => string.Equals(Norm(a), Norm(b), StringComparison.Ordinal);

        private static string Norm(string? value)
            => string.IsNullOrWhiteSpace(value) ? "" : value.Trim();

        public void FreezeTmMapping(int fromTmTarget, string? fromTmStorage, int toTmTarget, string? toTmStorage)
        {
            EnsureNotClosed();
            FromTmTarget = fromTmTarget;
            // TM storage/port 可空：无值时冻结为空串，发给 TM 的 storage 允许 ""
            FromTmStorage = fromTmStorage?.Trim() ?? "";
            ToTmTarget = toTmTarget;
            ToTmStorage = toTmStorage?.Trim() ?? "";
        }

        public void FreezeOptionCodes(string fetchOptionCode, string putOptionCode)
        {
            EnsureNotClosed();
            FetchOptionCode = Check.NotNullOrWhiteSpace(fetchOptionCode, nameof(fetchOptionCode), 64);
            PutOptionCode = Check.NotNullOrWhiteSpace(putOptionCode, nameof(putOptionCode), 64);
        }

        public void FreezeOptionSchema(string schemaCode, int schemaVersion)
        {
            EnsureNotClosed();
            OptionCodeSchemaCode = Check.NotNullOrWhiteSpace(schemaCode, nameof(schemaCode), 32);
            if (schemaVersion <= 0)
            {
                throw new BusinessException("RCS:InvalidOptionCodeSchemaVersion")
                    .WithData("Version", schemaVersion);
            }

            OptionCodeSchemaVersion = schemaVersion;
        }

        public string GetOptionCode(string leg)
        => leg == TaskLegs.Fetch ? FetchOptionCode
        : leg == TaskLegs.Put ? PutOptionCode
        : throw new BusinessException("RCS:InvalidLeg").WithData("Leg", leg);

        //----------------- 聚合行为 ---------------
        public void MarkRunning()
        {
            if (TaskLifecycleStatus == TaskLifecycleStatus.Running)
            {
                return; // 幂等
            }
            EnsureNotClosed();
            TaskLifecycleStatus = TaskLifecycleStatus.Running;
        }

        public void SetWaiting(string eventName, string? leg)
        {
            EnsureNotClosed();
            if(string.IsNullOrWhiteSpace(eventName))
            {
                throw new BusinessException("RCS:InvalidEventName")
                .WithData("EventName", eventName);
            }
            WaitingEvent = eventName;
            ActiveLeg = leg;
        }

        public void ClearWaiting()
        {
            WaitingEvent = null;
            ActiveLeg = null;
        }

        public void AdvanceStep()
        {
            EnsureNotClosed();
            StepIndex++;
            ClearWaiting();
        }

        public void AssignSerial(string leg, string serial)
        {
            EnsureNotClosed();
            if(string.IsNullOrWhiteSpace(serial))
            {
                throw new BusinessException("RCS:InvalidSerial")
                .WithData("Serial", serial);
            }
            if(leg == TaskLegs.Fetch)
            {
                FetchTaskSerial = serial;
            }
            else if(leg == TaskLegs.Put)
            {
                PutTaskSerial = serial;
            }
            else
            {
                throw new BusinessException("RCS:InvalidLeg")
                .WithData("Leg", leg);
            }
        }

        public void RememberAgv(string serial)
        {
            EnsureNotClosed();
            AgvSerial = serial;
        }

        public void MarkSucceeded()
        {
            EnsureNotClosed();
            if(TaskLifecycleStatus == TaskLifecycleStatus.Pending)
            {
                throw new BusinessException("RCS:TaskNotRunning")
                .WithData("TaskId", Id)
                .WithData("Status", TaskLifecycleStatus);
            }
            TaskLifecycleStatus = TaskLifecycleStatus.Succeeded;
            ClearWaiting();
            // 本地事件：MES Handler 过滤 Source；失败不上报故 MarkFailed 不发
            AddLocalEvent(new Mes.TaskLifecycleEndedEvent(Id, Source, TaskLifecycleStatus.Succeeded));
        }

        public void MarkFailed(string error)
        {
            EnsureNotClosed();
            if(TaskLifecycleStatus == TaskLifecycleStatus.Pending)
            {
                throw new BusinessException("RCS:TaskNotRunning")
                .WithData("TaskId", Id)
                .WithData("Status", TaskLifecycleStatus);
            }
            TaskLifecycleStatus = TaskLifecycleStatus.Failed;
            ClearWaiting();
            LastError = error;
        }

        /// <param name="cancelMessage">取消原因；写入 RCS-101 cancel_message，可空。</param>
        public void MarkCanceled(string? cancelMessage = null)
        {
            EnsureNotClosed();
            TaskLifecycleStatus = TaskLifecycleStatus.Canceled;
            ClearWaiting();
            if (!string.IsNullOrWhiteSpace(cancelMessage))
            {
                LastError = cancelMessage.Trim();
            }

            AddLocalEvent(new Mes.TaskLifecycleEndedEvent(
                Id,
                Source,
                TaskLifecycleStatus.Canceled,
                string.IsNullOrWhiteSpace(cancelMessage) ? null : cancelMessage.Trim()));
        }

        public string? ResolveLegBySerial(string taskSerial)
        {
            if (FetchTaskSerial == taskSerial) return TaskLegs.Fetch;
            if (PutTaskSerial == taskSerial) return TaskLegs.Put;
            return null;
        }


        private void EnsureNotClosed()
        {
            if(TaskLifecycleStatus == TaskLifecycleStatus.Succeeded || TaskLifecycleStatus == TaskLifecycleStatus.Failed || TaskLifecycleStatus == TaskLifecycleStatus.Canceled)
            {
                throw new BusinessException("RCS:TaskAlreadyClosed")
                .WithData("TaskId", Id)
                .WithData("Status", TaskLifecycleStatus);
            }
        }
    }
}