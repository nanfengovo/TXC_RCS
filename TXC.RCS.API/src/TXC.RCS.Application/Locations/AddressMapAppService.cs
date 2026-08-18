using System;
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
public class AddressMapAppService : RCSAppService, IAddressMapAppService
{
    private readonly IRepository<AddressMap, Guid> _repository;

    public AddressMapAppService(IRepository<AddressMap, Guid> repository)
    {
        _repository = repository;
    }

    [AllowAnonymous]
    public async Task<PagedResultDto<AddressMapDto>> GetListAsync(GetAddressMapListInput input)
    {
        var query = await _repository.GetQueryableAsync();
        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                x => x.AddressCode.Contains(input.Keyword!)
                     || (x.Remark != null && x.Remark.Contains(input.Keyword!)))
            .WhereIf(input.IsEnabled.HasValue, x => x.IsEnabled == input.IsEnabled!.Value);

        var total = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "AddressCode asc" : input.Sorting;
        query = query.OrderBy(sorting);

        var items = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<AddressMapDto>(total, items.Select(Map).ToList());
    }

    [AllowAnonymous]
    public async Task<AddressMapDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return Map(entity);
    }

    [AllowAnonymous]
    public async Task<AddressMapDto> CreateAsync(CreateAddressMapDto input)
    {
        var code = input.AddressCode.Trim();
        if (await _repository.AnyAsync(x => x.AddressCode == code))
        {
            throw new BusinessException("RCS:AddressMapDuplicate")
                .WithData("AddressCode", code);
        }

        var entity = new AddressMap(
            GuidGenerator.Create(),
            code,
            input.TmTarget,
            input.TmStorage?.Trim() ?? string.Empty,
            input.Remark?.Trim(),
            input.IsEnabled);

        await _repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }

    [AllowAnonymous]
    public async Task<AddressMapDto> UpdateAsync(Guid id, UpdateAddressMapDto input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Configure(
            input.TmTarget,
            input.TmStorage?.Trim(),
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

    private static AddressMapDto Map(AddressMap entity) => new()
    {
        Id = entity.Id,
        AddressCode = entity.AddressCode,
        TmTarget = entity.TmTarget,
        TmStorage = entity.TmStorage,
        Remark = entity.Remark,
        IsEnabled = entity.IsEnabled
    };
}
