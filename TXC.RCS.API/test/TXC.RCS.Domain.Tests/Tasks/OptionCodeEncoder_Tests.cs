using System;
using System.Collections.Generic;
using System.Linq;
using TXC.RCS.Locations;
using TXC.RCS.Tasks.Enums;
using TXC.RCS.Tasks.OptionCode;
using Volo.Abp;
using Xunit;

namespace TXC.RCS.Tasks.OptionCode;

public class OptionCodeEncoder_Tests
{
    private static readonly OptionCodeEncoder Encoder = new();

    [Fact]
    public void TxcDemo_Should_Pack_Lsb_Bytes()
    {
        var schema = TxcDemoSchema();
        var code = Encoder.Encode(schema, new Dictionary<string, int>
        {
            ["armSide"] = 1,
            ["agvSlot"] = 1,
            ["equipmentType"] = 1,
            ["pickPlace"] = 2,
            ["machineNo"] = 1,
            ["equipmentSlot"] = 2
        });

        Assert.Equal("257,16908801", code);
    }

    [Fact]
    public void UnevenBits_Should_Pack_Without_Assuming_8()
    {
        var schema = new OptionCodeSchema
        {
            Code = "uneven",
            Version = 1,
            Parts =
            [
                new OptionCodePart
                {
                    Key = "code1",
                    Width = 32,
                    Fields =
                    [
                        new OptionCodeField { Key = "cam", BitStart = 1, BitEnd = 8 },
                        new OptionCodeField { Key = "slot", BitStart = 9, BitEnd = 16 },
                        new OptionCodeField { Key = "box", BitStart = 17, BitEnd = 20 },
                        new OptionCodeField { Key = "idx", BitStart = 21, BitEnd = 32 }
                    ]
                }
            ]
        };

        var code = Encoder.Encode(schema, new Dictionary<string, int>
        {
            ["cam"] = 4,
            ["slot"] = 2,
            ["box"] = 3,
            ["idx"] = 1
        });

        uint expected = 4u | (2u << 8) | (3u << 16) | (1u << 20);
        Assert.Equal(expected.ToString(), code);
    }

    [Fact]
    public void Missing_Required_Should_Fail()
    {
        var schema = TxcDemoSchema();
        var ex = Assert.Throws<BusinessException>(() =>
            Encoder.Encode(schema, new Dictionary<string, int> { ["armSide"] = 1 }));
        Assert.Equal("RCS:OptionCodeFieldRequired", ex.Code);
    }

    [Fact]
    public void Assembler_Should_Fill_Leg_And_Master()
    {
        var from = new AddressMap(Guid.NewGuid(), "ERACK", 1, "");
        var to = new AddressMap(Guid.NewGuid(), "H044", 2, "");
        var args = new CreateTaskArgs
        {
            FromAddress = "ERACK",
            FromPort = "2",
            ToAddress = "H044",
            ToPort = "1",
            OptionFields = new Dictionary<string, int> { ["armSide"] = 1, ["agvSlot"] = 1 }
        };

        var assembler = new OptionCodeAssembler();
        var schema = TxcDemoSchema();
        var fetch = assembler.Assemble(schema, args, from, to, TaskLegs.Fetch, OptionCodeSourceKind.Manual);
        var put = assembler.Assemble(schema, args, from, to, TaskLegs.Put, OptionCodeSourceKind.Manual);

        Assert.Equal(2, fetch["pickPlace"]);
        Assert.Equal(1, fetch["equipmentType"]);
        Assert.Equal(2, fetch["equipmentSlot"]);
        Assert.Equal(1, put["equipmentSlot"]);
        Assert.Equal(1, put["pickPlace"]);
        Assert.Equal(3, put["equipmentType"]);
        Assert.NotEqual(
            Encoder.Encode(schema, fetch),
            Encoder.Encode(schema, put));
    }

    private static OptionCodeSchema TxcDemoSchema()
    {
        var assembly = typeof(EmbeddedOptionCodeSchemaStore).Assembly;
        var name = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith("txc_demo.v1.json", StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new System.IO.StreamReader(stream);
        return System.Text.Json.JsonSerializer.Deserialize<OptionCodeSchema>(reader.ReadToEnd(), new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        })!;
    }
}
