using System;
using Volo.Abp.Application.Dtos;

namespace TXC.RCS.Locations;

public class AddressMapDto : EntityDto<Guid>
{
    public string AddressCode { get; set; } = null!;
    public int TmTarget { get; set; }
    public string? TmStorage { get; set; }
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateAddressMapDto
{
    public string AddressCode { get; set; } = null!;
    public int TmTarget { get; set; }
    public string? TmStorage { get; set; }
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class UpdateAddressMapDto
{
    public int TmTarget { get; set; }
    public string? TmStorage { get; set; }
    public string? Remark { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public class GetAddressMapListInput : PagedAndSortedResultRequestDto
{
    public string? Keyword { get; set; }
    public bool? IsEnabled { get; set; }
}
