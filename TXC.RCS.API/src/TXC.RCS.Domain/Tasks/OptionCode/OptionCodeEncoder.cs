namespace TXC.RCS.Tasks.OptionCode;

public interface IOptionCodeEncoder
{
    string Encode(OptionCodeSchema schema, IReadOnlyDictionary<string, int> fields);
}

public class OptionCodeEncoder : IOptionCodeEncoder, ITransientDependency
{
    public string Encode(OptionCodeSchema schema, IReadOnlyDictionary<string, int> fields)
    {
        var parts = new List<string>(schema.Parts.Count);
        foreach (var part in schema.Parts)
        {
            uint word = 0;
            foreach (var f in part.Fields)
            {
                if (f.Reserved)
                {
                    continue;
                }

                if (!fields.TryGetValue(f.Key, out var raw))
                {
                    if (f.Required)
                    {
                        throw new BusinessException("RCS:OptionCodeFieldRequired")
                            .WithData("Field", f.Key)
                            .WithData("Schema", schema.Code);
                    }

                    continue;
                }

                if (raw < 0 || (uint)raw > f.Mask)
                {
                    throw new BusinessException("RCS:OptionCodeValueOutOfRange")
                        .WithData("Field", f.Key)
                        .WithData("Value", raw);
                }

                if (f.Min is int min && raw < min)
                {
                    throw new BusinessException("RCS:OptionCodeValueOutOfRange").WithData("Field", f.Key);
                }

                if (f.Max is int max && raw > max)
                {
                    throw new BusinessException("RCS:OptionCodeValueOutOfRange").WithData("Field", f.Key);
                }

                if (f.Enum is { Count: > 0 } && !f.Enum.ContainsKey(raw.ToString()))
                {
                    throw new BusinessException("RCS:OptionCodeEnumInvalid").WithData("Field", f.Key);
                }

                word |= ((uint)raw & f.Mask) << f.Shift;
            }

            parts.Add(word.ToString());
        }

        return string.Join(schema.Wire.Join, parts);
    }
}
