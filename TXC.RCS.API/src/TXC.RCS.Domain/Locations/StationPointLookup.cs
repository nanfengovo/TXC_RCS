using System.Threading;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace TXC.RCS.Locations;

public class StationPointLookup : DomainService, IStationPointLookup, ITransientDependency
{
    private readonly IRepository<StationPoint, Guid> _points;

    public StationPointLookup(IRepository<StationPoint, Guid> points)
    {
        _points = points;
    }

    public async Task<IReadOnlyDictionary<string, int>> GetMasterValuesAsync(
        string addressCode,
        string? port,
        CancellationToken ct = default)
    {
        var address = addressCode.Trim();
        var portKey = port?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(portKey))
        {
            throw new BusinessException("RCS:StationPointPortRequired")
                .WithData("Address", address);
        }

        var point = await _points.FirstOrDefaultAsync(
            x => x.AddressCode == address && x.Port == portKey && x.IsEnabled,
            cancellationToken: ct);

        if (point == null)
        {
            throw new BusinessException("RCS:StationPointNotFound")
                .WithData("Address", address)
                .WithData("Port", portKey);
        }

        return point.GetMasterValues();
    }
}
