using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.Workflow;
using Xunit;

namespace TXC.RCS.Tasks;

public class TaskDo_MesIdempotency_Tests
{
    [Fact]
    public void MatchesMesDispatch_Should_Ignore_Whitespace_And_Empty()
    {
        var task = TaskDo.Create(
            "20231106111111001",
            null,
            TaskSource.Mes,
            new CreateTaskArgs
            {
                FromAddress = "ERACK",
                FromPort = "1",
                ToAddress = "H044",
                ToPort = "2",
                ContainerId = "C1",
                LotId = "L1"
            },
            WorkflowTemplateCatalog.FetchPutCode,
            WorkflowTemplateCatalog.FetchPutVersion);

        Assert.True(task.MatchesMesDispatch(new CreateTaskArgs
        {
            FromAddress = "ERACK",
            FromPort = "1",
            ToAddress = "H044",
            ToPort = "2",
            ContainerId = "C1",
            LotId = "L1"
        }));

        Assert.False(task.MatchesMesDispatch(new CreateTaskArgs
        {
            FromAddress = "ERACK",
            FromPort = "1",
            ToAddress = "H044",
            ToPort = "9",
            ContainerId = "C1",
            LotId = "L1"
        }));
    }

    [Fact]
    public void DescribeMesDispatchDiff_Should_List_Changed_Fields()
    {
        var task = TaskDo.Create(
            "J1",
            null,
            TaskSource.Mes,
            new CreateTaskArgs
            {
                FromAddress = "A",
                ToAddress = "B",
                ContainerId = "C",
                LotId = "L"
            },
            WorkflowTemplateCatalog.FetchPutCode,
            WorkflowTemplateCatalog.FetchPutVersion);

        var diff = task.DescribeMesDispatchDiff(new CreateTaskArgs
        {
            FromAddress = "A",
            ToAddress = "B2",
            ContainerId = "C2",
            LotId = "L"
        });

        Assert.Contains("终点地址", diff);
        Assert.Contains("料盒", diff);
    }
}
