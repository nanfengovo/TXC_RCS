using System.Threading.Tasks;
using TXC.RCS.Tasks;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.EventConst;
using TXC.RCS.Tasks.Workflow;
using TXC.RCS.Tasks.Workflow.Activities;
using Xunit;

namespace TXC.RCS.Tasks;

public class TaskWorkflow_Tests
{
    private static TaskWorkflow CreateWorkflow()
    {
        var activities = new IWorkflowActivity[]
        {
            new FakeTmDispatchActivity()
        };

        return new TaskWorkflow(
            new InMemoryWorkflowTemplateResolver(),
            new WorkflowActivityExecutor(activities));
    }

    private static TaskDo NewTask()
    {
        return TaskDo.Create(
            "T001",
            orderId: null,
            args: new CreateTaskArgs { FromAddress = "A1", ToAddress = "B1" },
            templateCode: WorkflowTemplateCatalog.FetchPutCode,
            templateVersion: WorkflowTemplateCatalog.FetchPutVersion);
    }

    [Fact]
    public async Task Start_Should_Dispatch_And_Wait_Fetch_Finished()
    {
        var wf = CreateWorkflow();
        var task = NewTask();

        await wf.StartAsync(task);

        Assert.Equal(TaskLifecycleStatus.Running, task.TaskLifecycleStatus);
        Assert.Equal("T001-FETCH", task.FetchTaskSerial);
        Assert.Equal("T001-PUT", task.PutTaskSerial);
        Assert.Equal(TaskEvents.Finished, task.WaitingEvent);
        Assert.Equal(TaskLegs.Fetch, task.ActiveLeg);
        Assert.Equal(1, task.StepIndex);
    }

    [Fact]
    public async Task Full_HappyPath_Should_Succeed()
    {
        var wf = CreateWorkflow();
        var task = NewTask();

        await wf.StartAsync(task);

        await wf.SignalAsync(task, new TaskSignal
        {
            Event = TaskEvents.Finished,
            TaskSerial = task.FetchTaskSerial  // 靠 Serial 反查 Leg
        });

        Assert.Equal(TaskEvents.Finished, task.WaitingEvent);
        Assert.Equal(TaskLegs.Put, task.ActiveLeg);

        await wf.SignalAsync(task, new TaskSignal
        {
            Event = TaskEvents.Finished,
            TaskSerial = task.PutTaskSerial
        });

        Assert.Equal(TaskLifecycleStatus.Succeeded, task.TaskLifecycleStatus);
        Assert.Null(task.WaitingEvent);
    }
}