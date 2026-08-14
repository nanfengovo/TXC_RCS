using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TXC.RCS.Tasks.OptionCode;

public interface IOptionCodeSchemaStore
{
    OptionCodeSchema GetPublished();

    OptionCodeSchema Get(string code, int version);
}

public class EmbeddedOptionCodeSchemaStore : IOptionCodeSchemaStore, ISingletonDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly OptionCodeOptions _opt;
    private readonly IReadOnlyDictionary<(string Code, int Version), OptionCodeSchema> _all;

    public EmbeddedOptionCodeSchemaStore(IOptions<OptionCodeOptions> opt)
    {
        _opt = opt.Value;
        _all = LoadEmbedded();
    }

    public OptionCodeSchema GetPublished() => Get(_opt.SchemaCode, _opt.SchemaVersion);

    public OptionCodeSchema Get(string code, int version)
    {
        if (!_all.TryGetValue((code, version), out var schema))
        {
            throw new BusinessException("RCS:OptionCodeSchemaNotFound")
                .WithData("Code", code)
                .WithData("Version", version);
        }

        return schema;
    }

    private static IReadOnlyDictionary<(string, int), OptionCodeSchema> LoadEmbedded()
    {
        var assembly = typeof(EmbeddedOptionCodeSchemaStore).Assembly;
        var result = new Dictionary<(string, int), OptionCodeSchema>();

        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (!name.Contains(".OptionCode.Schemas.", StringComparison.Ordinal) ||
                !name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            using var stream = assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Missing embedded schema: {name}");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var schema = JsonSerializer.Deserialize<OptionCodeSchema>(json, JsonOptions)
                ?? throw new InvalidOperationException($"Invalid schema JSON: {name}");
            Validate(schema);
            result[(schema.Code, schema.Version)] = schema;
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("No OptionCode schema JSON was embedded.");
        }

        return result;
    }

    internal static void Validate(OptionCodeSchema schema)
    {
        foreach (var part in schema.Parts)
        {
            var occupied = new bool[Math.Max(part.Width, 32) + 1];
            foreach (var f in part.Fields)
            {
                if (f.BitStart < 1 || f.BitEnd > part.Width || f.BitStart > f.BitEnd)
                {
                    throw new BusinessException("RCS:OptionCodeSchemaInvalid")
                        .WithData("Part", part.Key)
                        .WithData("Field", f.Key);
                }

                for (var bit = f.BitStart; bit <= f.BitEnd; bit++)
                {
                    if (occupied[bit])
                    {
                        throw new BusinessException("RCS:OptionCodeSchemaOverlap")
                            .WithData("Part", part.Key)
                            .WithData("Bit", bit);
                    }

                    occupied[bit] = true;
                }
            }
        }
    }
}
