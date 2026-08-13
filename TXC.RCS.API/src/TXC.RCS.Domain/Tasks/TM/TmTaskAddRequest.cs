using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TXC.RCS.Tasks.TM;

public class TmTaskAddRequest
{
    /// <summary>组合任务子任务数量</summary>
    [JsonPropertyName("bulk_task_count")]
    public int BulkTaskCount { get; set; }

    /// <summary>组合任务类型名，即要求配置任务步 xml 的文件名</summary>
    [JsonPropertyName("bulk_task_type")]
    public string BulkTaskType { get; set; } = "task";

    /// <summary>子任务数组</summary>
    [JsonPropertyName("sub_task")]
    public List<TmSubTaskDto> SubTask { get; set; } = new();
}

public class TmSubTaskDto
{
    [JsonPropertyName("AGV_serial")]
    public int AgvSerial { get; set; }

    [JsonPropertyName("robot_type")]
    public string RobotType { get; set; } = "";

    [JsonPropertyName("area_property")]
    public List<string> AreaProperty { get; set; } = new();

    [JsonPropertyName("cargo_id")]
    public string CargoId { get; set; } = "";

    [JsonPropertyName("complete_time")]
    public string CompleteTime { get; set; } = "";

    [JsonPropertyName("succession")]
    public int Succession { get; set; }

    [JsonPropertyName("pre_report")]
    public string PreReport { get; set; } = "0";

    [JsonPropertyName("priority")]
    public int Priority { get; set; } = 1;

    [JsonPropertyName("goal_action")]
    public int GoalAction { get; set; }

    [JsonPropertyName("mark")]
    public string Mark { get; set; } = "";

    [JsonPropertyName("option_code")]
    public string OptionCode { get; set; } = "0,0";

    [JsonPropertyName("target")]
    public int Target { get; set; }

    [JsonPropertyName("storage")]
    public string Storage { get; set; } = "";

    [JsonPropertyName("task_serial")]
    public string TaskSerial { get; set; } = "";

    [JsonPropertyName("task_type")]
    public string TaskType { get; set; } = "";
}
