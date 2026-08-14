using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace TXC.RCS.Locations;

public class AddressMapDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AddressMap, Guid> _addressMaps;
    private readonly IGuidGenerator _guidGenerator;

    public AddressMapDataSeedContributor(
        IRepository<AddressMap, Guid> addressMaps,
        IGuidGenerator guidGenerator)
    {
        _addressMaps = addressMaps;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // 幂等：已有数据就跳过（或按 AddressCode 逐条 Upsert）
        if (await _addressMaps.GetCountAsync() > 0)
        {
            return;
        }

        await InsertIfNotExistsAsync("ERACK", tmTarget: 1, tmStorage: "", remark: "Erack 1");
        await InsertIfNotExistsAsync("H044", tmTarget: 2, tmStorage: "", remark: "");
        await InsertIfNotExistsAsync("H099", tmTarget: 3, tmStorage: "", remark: "机台示例");
    }

    private async Task InsertIfNotExistsAsync(
        string addressCode, int tmTarget, string tmStorage, string? remark)
    {
        if (await _addressMaps.AnyAsync(x => x.AddressCode == addressCode))
        {
            return;
        }

        // 注意：你的 AddressMap 构造函数没设 Id，插入前要赋值
       var map = new AddressMap(_guidGenerator.Create(), addressCode, tmTarget, tmStorage, remark);
        await _addressMaps.InsertAsync(map, autoSave: true);
    }
}