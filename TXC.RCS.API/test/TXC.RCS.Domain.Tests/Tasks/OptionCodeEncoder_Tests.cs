using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            ["agvSlot"] = 0,
            ["equipmentType"] = 1,
            ["pickPlace"] = 2,
            ["machineNo"] = 1,
            ["equipmentSlot"] = 2
        });

        Assert.Equal("1,16908801", code);
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
    public async Task Assembler_Should_Fill_From_Point_And_Leg()
    {
        var lookup = new FakePointLookup();
        var assembler = new OptionCodeAssembler(lookup);
        var schema = TxcDemoSchema();
        var args = new CreateTaskArgs
        {
            FromAddress = "ERACK",
            FromPort = "2",
            ToAddress = "H044",
            ToPort = "1"
        };

        var fetch = await assembler.AssembleAsync(schema, args, "ERACK", "2", TaskLegs.Fetch);
        var put = await assembler.AssembleAsync(schema, args, "H044", "1", TaskLegs.Put);

        Assert.Equal(1, fetch["armSide"]);
        Assert.Equal(0, fetch["agvSlot"]);
        Assert.Equal(2, fetch["pickPlace"]);
        Assert.Equal(1, fetch["equipmentType"]);
        Assert.Equal(2, fetch["equipmentSlot"]);
        Assert.Equal(2, put["armSide"]);
        Assert.Equal(1, put["pickPlace"]);
        Assert.Equal(3, put["equipmentType"]);
        Assert.Equal(1, put["equipmentSlot"]);
        Assert.NotEqual(Encoder.Encode(schema, fetch), Encoder.Encode(schema, put));
    }

    [Fact]
    public async Task Assembler_Should_Ignore_OptionFields_Overwrite_Of_Master()
    {
        var lookup = new FakePointLookup();
        var assembler = new OptionCodeAssembler(lookup);
        var schema = TxcDemoSchema();
        var args = new CreateTaskArgs
        {
            FromAddress = "ERACK",
            FromPort = "2",
            ToAddress = "H044",
            ToPort = "1",
            OptionFields = new Dictionary<string, int>
            {
                ["equipmentType"] = 99,
                ["pickPlace"] = 99,
                ["armSide"] = 99
            }
        };

        var fetch = await assembler.AssembleAsync(schema, args, "ERACK", "2", TaskLegs.Fetch);
        Assert.Equal(1, fetch["equipmentType"]);
        Assert.Equal(2, fetch["pickPlace"]);
        Assert.Equal(1, fetch["armSide"]);
    }

    [Fact]
    public async Task Assembler_Should_Fail_When_Point_Missing()
    {
        var assembler = new OptionCodeAssembler(new FakePointLookup());
        var schema = TxcDemoSchema();
        var args = new CreateTaskArgs { FromAddress = "X", FromPort = "9", ToAddress = "Y", ToPort = "1" };
        var ex = await Assert.ThrowsAsync<BusinessException>(() =>
            assembler.AssembleAsync(schema, args, "X", "9", TaskLegs.Fetch));
        Assert.Equal("RCS:StationPointNotFound", ex.Code);
    }

    private sealed class FakePointLookup : IStationPointLookup
    {
        public Task<IReadOnlyDictionary<string, int>> GetMasterValuesAsync(
            string addressCode, string? port, CancellationToken ct = default)
        {
            var key = $"{addressCode}:{port}";
            IReadOnlyDictionary<string, int>? values = key switch
            {
                "ERACK:2" => new Dictionary<string, int>
                {
                    ["armSide"] = 1,
                    ["equipmentType"] = 1,
                    ["machineNo"] = 1
                },
                "H044:1" => new Dictionary<string, int>
                {
                    ["armSide"] = 2,
                    ["equipmentType"] = 3,
                    ["machineNo"] = 2
                },
                _ => null
            };

            if (values == null)
            {
                throw new BusinessException("RCS:StationPointNotFound")
                    .WithData("Address", addressCode)
                    .WithData("Port", port ?? "");
            }

            return Task.FromResult(values);
        }
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
