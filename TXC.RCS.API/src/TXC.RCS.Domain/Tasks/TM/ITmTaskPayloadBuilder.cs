using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Tasks.TM;

public interface ITmTaskPayloadBuilder
{
    TmDispatchBuildResult Build(TaskDo task);
}

public class DefaultTmTaskPayloadBuilder : ITmTaskPayloadBuilder, ITransientDependency
{
    public TmDispatchBuildResult Build(TaskDo task)
    {
        if (task.FromTmTarget <= 0 || task.ToTmTarget <= 0)
        {
            throw new BusinessException("RCS:TmMappingNotFrozen")
                .WithData("TaskId", task.Id);
        }

        var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        var fetchSerial = $"{task.Id}_GET_{stamp}";
        var putSerial = $"{task.Id}_PUT_{stamp}";

        var req = new TmTaskAddRequest
        {
            BulkTaskCount = 2,
            BulkTaskType = "task",
            SubTask =
            {
                new TmSubTaskDto
                {
                    GoalAction = 1,
                    Target = task.FromTmTarget,
                    Storage = task.FromTmStorage,
                    TaskSerial = fetchSerial,
                    TaskType = "PA_14_get",
                    CargoId = task.ContainerId ?? "",
                    OptionCode = task.FetchOptionCode,
                    Succession = 1
                },
                new TmSubTaskDto
                {
                    GoalAction = 2,
                    Target = task.ToTmTarget,
                    Storage = task.ToTmStorage,
                    TaskSerial = putSerial,
                    TaskType = "PA_14_put",
                    CargoId = task.ContainerId ?? "",
                    OptionCode = task.PutOptionCode,
                    Succession = 2
                }
            }
        };

        return new TmDispatchBuildResult(req, fetchSerial, putSerial);
    }
}
