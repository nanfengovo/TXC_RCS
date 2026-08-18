using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace TXC.RCS.Locations;

public class StationPointDto : EntityDto<Guid>
{
    public string AddressCode { get; set; } = null!;
    public string Port { get; set; } = null!;
    public Dictionary<string, int> MasterValues { get; set; } = new();
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateStationPointDto
{
    public string AddressCode { get; set; } = null!;
    public string Port { get; set; } = null!;
    public Dictionary<string, int>? MasterValues { get; set; }
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateStationPointDto
{
    public string AddressCode { get; set; } = null!;
    public string Port { get; set; } = null!;
    public Dictionary<string, int>? MasterValues { get; set; }
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class GetStationPointListInput : PagedAndSortedResultRequestDto
{
    public string? Keyword { get; set; }
    public string? AddressCode { get; set; }
    public bool? IsEnabled { get; set; }
}
