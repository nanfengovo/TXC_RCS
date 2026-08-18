using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Locations;

public interface IAddressMapAppService : IApplicationService
{
    Task<PagedResultDto<AddressMapDto>> GetListAsync(GetAddressMapListInput input);

    Task<AddressMapDto> GetAsync(Guid id);

    Task<AddressMapDto> CreateAsync(CreateAddressMapDto input);

    Task<AddressMapDto> UpdateAsync(Guid id, UpdateAddressMapDto input);

    Task DeleteAsync(Guid id);
}
