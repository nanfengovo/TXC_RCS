using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace TXC.RCS.Locations;

public class StationPointDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<StationPoint, Guid> _points;
    private readonly IGuidGenerator _guids;

    public StationPointDataSeedContributor(
        IRepository<StationPoint, Guid> points,
        IGuidGenerator guids)
    {
        _points = points;
        _guids = guids;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await UpsertAsync("ERACK", "1", new Dictionary<string, int>
        {
            ["armSide"] = 1,
            ["equipmentType"] = 1,
            ["machineNo"] = 1
        }, "Erack 口1");
        await UpsertAsync("ERACK", "2", new Dictionary<string, int>
        {
            ["armSide"] = 1,
            ["equipmentType"] = 1,
            ["machineNo"] = 1
        }, "Erack 口2");
        await UpsertAsync("H044", "1", new Dictionary<string, int>
        {
            ["armSide"] = 2,
            ["equipmentType"] = 3,
            ["machineNo"] = 2
        });
        await UpsertAsync("H044", "2", new Dictionary<string, int>
        {
            ["armSide"] = 1,
            ["equipmentType"] = 3,
            ["machineNo"] = 2
        });
        await UpsertAsync("H099", "1", new Dictionary<string, int>
        {
            ["armSide"] = 1,
            ["equipmentType"] = 2,
            ["machineNo"] = 3
        });
    }

    private async Task UpsertAsync(
        string address,
        string port,
        Dictionary<string, int> values,
        string? remark = null)
    {
        if (await _points.AnyAsync(x => x.AddressCode == address && x.Port == port))
        {
            return;
        }

        await _points.InsertAsync(
            new StationPoint(_guids.Create(), address, port, values, remark),
            autoSave: true);
    }
}
