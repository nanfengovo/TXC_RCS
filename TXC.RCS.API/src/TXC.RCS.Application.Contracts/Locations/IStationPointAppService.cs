using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Locations;

public interface IStationPointAppService : IApplicationService
{
    Task<PagedResultDto<StationPointDto>> GetListAsync(GetStationPointListInput input);

    Task<StationPointDto> GetAsync(Guid id);

    Task<StationPointDto> CreateAsync(CreateStationPointDto input);

    Task<StationPointDto> UpdateAsync(Guid id, UpdateStationPointDto input);

    Task DeleteAsync(Guid id);
}
