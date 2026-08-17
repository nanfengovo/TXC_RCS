using System.Text.Json.Serialization;

namespace TXC.RCS.Tasks.OptionCode;

public sealed class OptionCodeSchema
{
    public required string Code { get; init; }

    public required int Version { get; init; }

    public string Title { get; init; } = "";

    public OptionCodeWire Wire { get; init; } = new();

    public required IReadOnlyList<OptionCodePart> Parts { get; init; }
}

public sealed class OptionCodeWire
{
    public string Join { get; init; } = ",";

    public bool LsbBit1 { get; init; } = true;
}

public sealed class OptionCodePart
{
    public required string Key { get; init; }

    public string Label { get; init; } = "";

    public int Width { get; init; } = 32;

    public required IReadOnlyList<OptionCodeField> Fields { get; init; }
}

public sealed class OptionCodeField
{
    public required string Key { get; init; }

    public string Label { get; init; } = "";

    public required int BitStart { get; init; }

    public required int BitEnd { get; init; }

    public bool Required { get; init; }

    public bool Reserved { get; init; }

    /// <summary>args | master | leg | port | task | const</summary>
    public string Source { get; init; } = "args";

    /// <summary>source=const 时写入的固定值（如 AGV 库位恒 0）。</summary>
    public int? ConstValue { get; init; }

    /// <summary>source=task 时绑定 CreateTaskArgs 属性（如 fetchCount）。</summary>
    public string? Bind { get; init; }

    public int? Min { get; init; }

    public int? Max { get; init; }

    [JsonPropertyName("enum")]
    public IReadOnlyDictionary<string, string>? Enum { get; init; }

    public int Width => BitEnd - BitStart + 1;

    public int Shift => BitStart - 1;

    public uint Mask => Width >= 32 ? uint.MaxValue : (1u << Width) - 1;
}
