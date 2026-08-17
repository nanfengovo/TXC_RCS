using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace TXC.RCS.Tasks;

public class GetTaskListInput : PagedAndSortedResultRequestDto
{
    public string? Keyword { get; set; }
    public string? Source { get; set; }
    public string? LifecycleStatus { get; set; }
    public string? FromAddress { get; set; }
    public string? ToAddress { get; set; }
    public string? ContainerId { get; set; }
    public string? LotId { get; set; }
}

public class TaskInteractionLogDto
{
    public Guid Id { get; set; }
    public string TaskId { get; set; } = "";
    public string Category { get; set; } = "";
    public string EventName { get; set; } = "";
    public string? Leg { get; set; }
    public string? Message { get; set; }
    public string? DetailJson { get; set; }
    public bool Success { get; set; }
    public DateTime CreationTime { get; set; }
}

public class TaskTimelineStepDto
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public string? EventName { get; set; }
    public string? Leg { get; set; }
    /// <summary>done / current / pending / error / canceled</summary>
    public string Status { get; set; } = "pending";
    public DateTime? Time { get; set; }
}

public class TaskMonitorDetailDto
{
    public TaskDto Task { get; set; } = null!;
    public List<TaskTimelineStepDto> Timeline { get; set; } = [];
    public List<TaskInteractionLogDto> Logs { get; set; } = [];
}
