using System.Threading;

namespace TXC.RCS.Locations;

public interface IStationPointLookup
{
    /// <summary>按地址+口取 Schema master 字段。未配置则抛友好业务异常。</summary>
    Task<IReadOnlyDictionary<string, int>> GetMasterValuesAsync(
        string addressCode,
        string? port,
        CancellationToken ct = default);
}
