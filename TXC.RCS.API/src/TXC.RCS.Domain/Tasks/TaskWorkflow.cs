using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TXC.RCS.Tasks.Workflow;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Distributed;

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

        public async Task<IReadOnlyDictionary<string, string>> SignalAsync(TaskDo task, TaskSignal signal, CancellationToken ct = default)
        {
            if (task.TaskLifecycleStatus is TaskLifecycleStatus.Succeeded
                or TaskLifecycleStatus.Failed
                or TaskLifecycleStatus.Canceled)
                return Empty;

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
            IReadOnlyDictionary<string, string> response = Empty;

            if (!string.IsNullOrWhiteSpace(effective.AgvSerial))
                task.RememberAgv(effective.AgvSerial!);

            var def = await _templates.ResolveAsync(task, ct);
            var step = def.Steps[task.StepIndex];

            if (step.Wait == null || !step.Wait.Matches(effective))
                return Empty; // 不匹配：忽略，别推进

            if (!string.IsNullOrWhiteSpace(step.Activity))
                response = await _activities.ExecuteAsync(step.Activity, task, effective, ct);

            task.AdvanceStep();
            await RunFromCurrentAsync(task, effective, ct);
            return response;
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