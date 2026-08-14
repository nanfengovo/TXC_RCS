using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp.Domain.Services;
using TXC.RCS.Tasks.EventConst;

namespace TXC.RCS.Tasks
{
    public class TaskWorkflow : DomainService, ITaskWorkflow
    {
        private readonly IWorkflowTemplateResolver _templates;
        private readonly IWorkflowActivityExecutor _activities;
        private static readonly IReadOnlyDictionary<string, string> Empty =new Dictionary<string, string>();

        public TaskWorkflow(IWorkflowTemplateResolver templates, IWorkflowActivityExecutor activities)
        {
            _templates = templates;
            _activities = activities;
        }

        public async Task StartAsync(TaskDo task, CancellationToken ct = default)
        {
            if (task.TaskLifecycleStatus != TaskLifecycleStatus.Pending)
                throw new BusinessException("RCS:InvalidStart");

            task.MarkRunning();
            await RunFromCurrentAsync(task, incoming: null, ct);
        }

        public async Task<WorkflowSignalResult> SignalAsync(TaskDo task, TaskSignal signal, CancellationToken ct = default)
        {
            // 已结束：幂等成功，停 TM 重试
            if (task.TaskLifecycleStatus is TaskLifecycleStatus.Succeeded
                or TaskLifecycleStatus.Failed
                or TaskLifecycleStatus.Canceled)
            {
                return ReplayIfPermit(task, signal) ?? WorkflowSignalResult.Ok(Empty);
            }

            var leg = signal.Leg;
            if (leg == null && !string.IsNullOrWhiteSpace(signal.TaskSerial))
                leg = task.ResolveLegBySerial(signal.TaskSerial!);

            var effective = new TaskSignal
            {
                Event = signal.Event,
                Leg = leg,
                TaskSerial = signal.TaskSerial,
                AgvSerial = signal.AgvSerial
            };

            var def = await _templates.ResolveAsync(task, ct);

            if (task.StepIndex < 0 || task.StepIndex >= def.Steps.Count)
                throw new BusinessException("RCS:InvalidStep").WithData("StepIndex", task.StepIndex);

            var step = def.Steps[task.StepIndex];

            // 当前步匹配 → 推进
            if (step.Wait != null && step.Wait.Matches(effective))
            {
                if (!string.IsNullOrWhiteSpace(effective.AgvSerial))
                    task.RememberAgv(effective.AgvSerial!);

                IReadOnlyDictionary<string, string> response = Empty;
                if (!string.IsNullOrWhiteSpace(step.Activity))
                    response = await _activities.ExecuteAsync(step.Activity, task, effective, ct);

                task.AdvanceStep();
                await RunFromCurrentAsync(task, effective, ct);
                return WorkflowSignalResult.Ok(response);
            }

            // 当前步之前已出现过同一 Wait → 落后/重复，不推进
            if (WasAlreadyConsumed(def, task.StepIndex, effective))
                return ReplayIfPermit(task, effective) ?? WorkflowSignalResult.Ok(Empty);

            // 超前乱序或错腿
            return WorkflowSignalResult.Reject(
                expectedEvent: step.Wait?.Event ?? task.WaitingEvent,
                expectedLeg: step.Wait?.Leg ?? task.ActiveLeg,
                actualEvent: effective.Event,
                actualLeg: effective.Leg);
        }

        private static bool WasAlreadyConsumed(
            WorkflowTemplateDefinition def,
            int currentStepIndex,
            TaskSignal signal)
        {
            for (var i = 0; i < currentStepIndex && i < def.Steps.Count; i++)
            {
                var wait = def.Steps[i].Wait;
                if (wait != null && wait.Matches(signal))
                    return true;
            }
            return false;
        }

        /// <summary>许可类重复：再给 option_code，避免 TM 重推 permit 时拿不到码。</summary>
        private static WorkflowSignalResult? ReplayIfPermit(TaskDo task, TaskSignal signal)
        {
            if (signal.Event != TaskEvents.PermitRequested)
                return null;

            var leg = signal.Leg
                ?? (!string.IsNullOrWhiteSpace(signal.TaskSerial)
                    ? task.ResolveLegBySerial(signal.TaskSerial!)
                    : task.ActiveLeg)
                ?? TaskLegs.Fetch;

            return WorkflowSignalResult.Ok(new Dictionary<string, string>
            {
                ["option_code"] = task.GetOptionCode(leg),
                ["task_serial"] = signal.TaskSerial ?? ""
            });
        }

        private async Task RunFromCurrentAsync(TaskDo task, TaskSignal? incoming, CancellationToken ct)
        {
            var def = await _templates.ResolveAsync(task, ct);

            while (task.StepIndex < def.Steps.Count)
            {
                var step = def.Steps[task.StepIndex];

                if (step.Wait != null)
                {
                    task.SetWaiting(step.Wait.Event, step.Wait.Leg);
                    return;
                }

                if (string.IsNullOrWhiteSpace(step.Activity))
                    throw new BusinessException("RCS:InvalidStep").WithData("StepId", step.Id);

                await _activities.ExecuteAsync(step.Activity, task, incoming, ct);
                task.AdvanceStep();
            }

            task.MarkSucceeded();
        }
    }
}