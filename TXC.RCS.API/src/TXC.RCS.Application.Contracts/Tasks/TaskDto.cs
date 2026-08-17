namespace TXC.RCS.Tasks;

/// <summary>
/// 任务对外快照（给前端 / Swagger / 联调）。
/// </summary>
public class TaskDto
{
    public string Id { get; set; } = "";
    public string Source { get; set; } = "";
    public string? LotId { get; set; }
    public string LifecycleStatus { get; set; } = "";
    public string? WaitingEvent { get; set; }
    public string? ActiveLeg { get; set; }
    public int StepIndex { get; set; }
    public string? FetchTaskSerial { get; set; }
    public string? PutTaskSerial { get; set; }
    public string? AgvSerial { get; set; }
    public string FromAddress { get; set; } = "";
    public string? FromPort { get; set; }
    public string? ToAddress { get; set; }
    public string? ToPort { get; set; }
    public string? ContainerId { get; set; }
    public string FetchOptionCode { get; set; } = "";
    public string PutOptionCode { get; set; } = "";
    public string? OptionCodeSchemaCode { get; set; }
    public int OptionCodeSchemaVersion { get; set; }
    public string? LastError { get; set; }
    public System.DateTime CreationTime { get; set; }
    public System.DateTime? LastModificationTime { get; set; }
}
