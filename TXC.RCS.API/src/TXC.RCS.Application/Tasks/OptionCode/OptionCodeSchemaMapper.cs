using System.Collections.Generic;
using System.Linq;
using TXC.RCS.Tasks.OptionCode;

namespace TXC.RCS.Tasks.OptionCode;

internal static class OptionCodeSchemaMapper
{
    public static PublishedOptionCodeSchemaDto ToPublishedDto(OptionCodeSchema schema)
    {
        var parts = schema.Parts.Select(p => new OptionCodePartDto
        {
            Key = p.Key,
            Label = p.Label,
            Width = p.Width,
            Fields = p.Fields.Select(f => new OptionCodeFieldDto
            {
                Key = f.Key,
                Label = f.Label,
                BitStart = f.BitStart,
                BitEnd = f.BitEnd,
                Required = f.Required,
                Reserved = f.Reserved,
                Source = f.Source,
                Min = f.Min,
                Max = f.Max,
                Enum = f.Enum is null ? null : new Dictionary<string, string>(f.Enum)
            }).ToList()
        }).ToList();

        var inputs = schema.Parts
            .SelectMany(p => p.Fields)
            .Where(f => !f.Reserved && IsCallerInput(f.Source))
            .Select(ToInput)
            .ToList();

        return new PublishedOptionCodeSchemaDto
        {
            Code = schema.Code,
            Version = schema.Version,
            Title = schema.Title,
            Parts = parts,
            Inputs = inputs
        };
    }

    private static bool IsCallerInput(string source) =>
        source is "args" or "port" or "task";

    private static OptionCodeInputDto ToInput(OptionCodeField f)
    {
        var dto = new OptionCodeInputDto
        {
            Key = f.Key,
            Label = f.Label,
            Source = f.Source,
            Required = f.Required,
            Min = f.Min,
            Max = f.Max,
            Enum = f.Enum is null ? null : new Dictionary<string, string>(f.Enum)
        };

        switch (f.Source)
        {
            case "port":
                dto.Bind = "fromPort|toPort";
                dto.BindFetch = "fromPort";
                dto.BindPut = "toPort";
                break;
            case "task":
                dto.Bind = f.Bind ?? f.Key;
                break;
            default:
                dto.Bind = $"optionFields.{f.Key}";
                break;
        }

        return dto;
    }
}
