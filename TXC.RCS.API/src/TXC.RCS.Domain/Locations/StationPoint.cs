using System.Text.Json;
using Volo.Abp.Domain.Entities;

namespace TXC.RCS.Locations;

/// <summary>
/// 工艺点位：与 AddressMap（TM 站点）分离。主数据按 Schema field.key 存 JSON。
/// </summary>
public class StationPoint : Entity<Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string AddressCode { get; private set; } = null!;

    public string Port { get; private set; } = null!;

    public string MasterValuesJson { get; private set; } = "{}";

    public string? Remark { get; private set; }

    public bool IsEnabled { get; private set; }

    protected StationPoint()
    {
    }

    public StationPoint(
        Guid id,
        string addressCode,
        string port,
        IReadOnlyDictionary<string, int> masterValues,
        string? remark = null,
        bool isEnabled = true)
        : base(id)
    {
        AddressCode = Check.NotNullOrWhiteSpace(addressCode, nameof(addressCode), 64);
        Port = Check.NotNullOrWhiteSpace(port, nameof(port), 16);
        SetMasterValues(masterValues);
        Remark = remark;
        IsEnabled = isEnabled;
    }

    public void SetMasterValues(IReadOnlyDictionary<string, int> masterValues)
    {
        MasterValuesJson = JsonSerializer.Serialize(masterValues, JsonOptions);
    }

    public IReadOnlyDictionary<string, int> GetMasterValues()
    {
        if (string.IsNullOrWhiteSpace(MasterValuesJson))
        {
            return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        var parsed = JsonSerializer.Deserialize<Dictionary<string, int>>(MasterValuesJson, JsonOptions);
        return parsed ?? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    public void Configure(
        string addressCode,
        string port,
        IReadOnlyDictionary<string, int> masterValues,
        string? remark,
        bool isEnabled)
    {
        AddressCode = Check.NotNullOrWhiteSpace(addressCode, nameof(addressCode), 64);
        Port = Check.NotNullOrWhiteSpace(port, nameof(port), 16);
        SetMasterValues(masterValues);
        Remark = remark;
        IsEnabled = isEnabled;
    }
}
