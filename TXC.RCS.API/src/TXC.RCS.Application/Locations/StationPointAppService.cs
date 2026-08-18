using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TXC.RCS.Swagger;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace TXC.RCS.Locations;

[ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
public class StationPointAppService : RCSAppService, IStationPointAppService
{
    private readonly IRepository<StationPoint, Guid> _repository;

    public StationPointAppService(IRepository<StationPoint, Guid> repository)
    {
        _repository = repository;
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<StationPointDto>> GetListAsync(GetStationPointListInput input)
    {
        var query = await _repository.GetQueryableAsync();
        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.AddressCode.Contains(input.Keyword!)
                     || x.Port.Contains(input.Keyword!)
                     || (x.Remark != null && x.Remark.Contains(input.Keyword!)))
            .WhereIf(!string.IsNullOrWhiteSpace(input.AddressCode),
                x => x.AddressCode == input.AddressCode)
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled!.Value);

        var total = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "AddressCode asc, Port asc" : input.Sorting;
        query = query.OrderBy(sorting);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<StationPointDto>(total, items.Select(Map).ToList());
    }

    [AllowAnonymous]
    public async Task<StationPointDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    [AllowAnonymous]
    public async Task<StationPointDto> CreateAsync(CreateStationPointDto input)
    {
        var address = input.AddressCode.Trim();
        var port = input.Port.Trim();
        await EnsureUniqueAsync(address, port);

        var entity = new StationPoint(
            GuidGenerator.Create(),
            address,
            port,
            NormalizeMasterValues(input.MasterValues),
            input.Remark?.Trim(),
            input.IsEnabled);

        await _repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }

    [AllowAnonymous]
    public async Task<StationPointDto> UpdateAsync(Guid id, UpdateStationPointDto input)
    {
        var entity = await _repository.GetAsync(id);
        var address = input.AddressCode.Trim();
        var port = input.Port.Trim();

        if (!string.Equals(entity.AddressCode, address, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(entity.Port, port, StringComparison.OrdinalIgnoreCase))
        {
            await EnsureUniqueAsync(address, port, id);
        }

        entity.Configure(
            address,
            port,
            NormalizeMasterValues(input.MasterValues),
            input.Remark?.Trim(),
            input.IsEnabled);

        await _repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }

    [AllowAnonymous]
    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    private async Task EnsureUniqueAsync(string addressCode, string port, Guid? excludeId = null)
    {
        if (await _repository.AnyAsync(x =>
                x.AddressCode == addressCode
                && x.Port == port
                && (!excludeId.HasValue || x.Id != excludeId.Value)))
        {
            throw new BusinessException("RCS:StationPointDuplicate")
                .WithData("AddressCode", addressCode)
                .WithData("Port", port);
        }
    }

    private static Dictionary<string, int> NormalizeMasterValues(Dictionary<string, int>? values)
    {
        if (values == null || values.Count == 0)
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        return values.ToDictionary(
            x => x.Key,
            x => x.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    private static StationPointDto Map(StationPoint entity)
    {
        var dto = new StationPointDto
        {
            Id = entity.Id,
            AddressCode = entity.AddressCode,
            Port = entity.Port,
            Remark = entity.Remark,
            IsEnabled = entity.IsEnabled
        };

        foreach (var pair in entity.GetMasterValues())
        {
            dto.MasterValues[pair.Key] = pair.Value;
        }

        return dto;
    }
}
