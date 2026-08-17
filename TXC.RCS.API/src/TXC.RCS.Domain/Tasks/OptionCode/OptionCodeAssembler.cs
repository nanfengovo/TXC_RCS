using System.Threading;
using TXC.RCS.Locations;

namespace TXC.RCS.Tasks.OptionCode;

public interface IOptionCodeAssembler
{
    Task<IReadOnlyDictionary<string, int>> AssembleAsync(
        OptionCodeSchema schema,
        CreateTaskArgs args,
        string address,
        string? port,
        string leg,
        CancellationToken ct = default);
}

public class OptionCodeAssembler : IOptionCodeAssembler, ITransientDependency
{
    private readonly IStationPointLookup _points;

    public OptionCodeAssembler(IStationPointLookup points)
    {
        _points = points;
    }

    public async Task<IReadOnlyDictionary<string, int>> AssembleAsync(
        OptionCodeSchema schema,
        CreateTaskArgs args,
        string address,
        string? port,
        string leg,
        CancellationToken ct = default)
    {
        var fields = schema.Parts.SelectMany(p => p.Fields).Where(f => !f.Reserved).ToList();
        IReadOnlyDictionary<string, int>? master = null;
        if (fields.Any(f => f.Source == "master"))
        {
            master = await _points.GetMasterValuesAsync(address, port, ct);
        }

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var f in fields)
        {
            switch (f.Source)
            {
                case "const":
                    if (f.ConstValue is int c)
                    {
                        map[f.Key] = c;
                    }

                    break;
                case "leg":
                    map[f.Key] = leg == TaskLegs.Fetch ? 2 : 1;
                    break;
                case "port":
                    if (int.TryParse(port, out var slot))
                    {
                        map[f.Key] = slot;
                    }

                    break;
                case "master":
                    if (master != null && TryGetIgnoreCase(master, f.Key, out var mv))
                    {
                        map[f.Key] = mv;
                    }
                    else if (f.Required)
                    {
                        throw new BusinessException("RCS:StationPointFieldMissing")
                            .WithData("Address", address)
                            .WithData("Port", port ?? "")
                            .WithData("Field", f.Key);
                    }

                    break;
                case "task":
                    var bind = f.Bind ?? f.Key;
                    if (bind.Equals("fetchCount", StringComparison.OrdinalIgnoreCase) && args.FetchCount is int fc)
                    {
                        map[f.Key] = fc;
                    }
                    else if (bind.Equals("putCount", StringComparison.OrdinalIgnoreCase) && args.PutCount is int pc)
                    {
                        map[f.Key] = pc;
                    }

                    break;
                case "args":
                    if (args.OptionFields != null && TryGetIgnoreCase(args.OptionFields, f.Key, out var av))
                    {
                        map[f.Key] = av;
                    }

                    break;
            }
        }

        return map;
    }

    private static bool TryGetIgnoreCase(IReadOnlyDictionary<string, int> source, string key, out int value)
    {
        if (source.TryGetValue(key, out value))
        {
            return true;
        }

        foreach (var kv in source)
        {
            if (string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = kv.Value;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
