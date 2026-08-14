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

        protected TaskDo(string id, Guid? orderId, CreateTaskArgs args, string templateCode, int templateVersion) : base(id)
        {
            Id = id;
            OrderId = orderId;
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

        public static TaskDo Create(string id, Guid? orderId, CreateTaskArgs args, string templateCode, int templateVersion)
        {
            Check.NotNullOrWhiteSpace(id, nameof(id));
            Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode));
            return new TaskDo(id, orderId, args, templateCode, templateVersion);
        }

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

        public void MarkCanceled()
        {
            EnsureNotClosed();
            TaskLifecycleStatus = TaskLifecycleStatus.Canceled;
            ClearWaiting();
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