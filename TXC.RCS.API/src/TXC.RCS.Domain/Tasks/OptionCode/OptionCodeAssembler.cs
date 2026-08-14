using TXC.RCS.Locations;

namespace TXC.RCS.Tasks.OptionCode;

public enum OptionCodeSourceKind
{
    Manual,
    Mes
}

public interface IOptionCodeAssembler
{
    IReadOnlyDictionary<string, int> Assemble(
        OptionCodeSchema schema,
        CreateTaskArgs args,
        AddressMap fromMap,
        AddressMap toMap,
        string leg,
        OptionCodeSourceKind source);
}

public class OptionCodeAssembler : IOptionCodeAssembler, ITransientDependency
{
    public IReadOnlyDictionary<string, int> Assemble(
        OptionCodeSchema schema,
        CreateTaskArgs args,
        AddressMap fromMap,
        AddressMap toMap,
        string leg,
        OptionCodeSourceKind source)
    {
        // S1：Mes 与 Manual 共用取值；以后在此补 Erack/MES，不要分叉 Encoder
        _ = source;

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (args.OptionFields != null)
        {
            foreach (var kv in args.OptionFields)
            {
                map[kv.Key] = kv.Value;
            }
        }

        var station = leg == TaskLegs.Fetch ? fromMap : toMap;
        var port = leg == TaskLegs.Fetch ? args.FromPort : args.ToPort;

        foreach (var field in schema.Parts.SelectMany(p => p.Fields))
        {
            if (field.Reserved || map.ContainsKey(field.Key))
            {
                continue;
            }

            switch (field.Source)
            {
                case "leg":
                    map[field.Key] = leg == TaskLegs.Fetch ? 2 : 1;
                    break;
                case "master":
                    if (field.Key == "equipmentType")
                    {
                        map[field.Key] = EquipmentTypeOf(station.AddressCode);
                    }
                    else if (field.Key == "machineNo")
                    {
                        map[field.Key] = station.TmTarget;
                    }

                    break;
                case "port":
                    if (int.TryParse(port, out var slot))
                    {
                        map[field.Key] = slot;
                    }

                    break;
                case "task":
                    if (field.Key == "fetchBoxCount" && args.FetchCount is int fc)
                    {
                        map[field.Key] = fc;
                    }

                    if (field.Key == "putBoxCount" && args.PutCount is int pc)
                    {
                        map[field.Key] = pc;
                    }

                    break;
            }
        }

        return map;
    }

    private static int EquipmentTypeOf(string addressCode) => addressCode.ToUpperInvariant() switch
    {
        "ERACK" => 1,
        "H099" => 2,
        "H044" => 3,
        _ => throw new BusinessException("RCS:OptionCodeEquipmentTypeUnknown")
            .WithData("Address", addressCode)
    };
}
