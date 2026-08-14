using System.Collections.Generic;

namespace TXC.RCS.Tasks.OptionCode;

/// <summary>
/// 当前厂 Published TaskCode Schema，供人工建单动态表单 / 位表展示。
/// </summary>
public class PublishedOptionCodeSchemaDto
{
    public string Code { get; set; } = "";

    public int Version { get; set; }

    public string Title { get; set; } = "";

    /// <summary>完整位段（含 master/leg，用于画位表）。</summary>
    public List<OptionCodePartDto> Parts { get; set; } = new();

    /// <summary>
    /// 需要调用方提供的输入。不含由地址/腿推导的字段。
    /// <c>source=args</c> → <c>optionFields</c>；<c>source=port</c> → <c>fromPort</c>/<c>toPort</c>。
    /// </summary>
    public List<OptionCodeInputDto> Inputs { get; set; } = new();
}

public class OptionCodePartDto
{
    public string Key { get; set; } = "";

    public string Label { get; set; } = "";

    public int Width { get; set; }

    public List<OptionCodeFieldDto> Fields { get; set; } = new();
}

public class OptionCodeFieldDto
{
    public string Key { get; set; } = "";

    public string Label { get; set; } = "";

    public int BitStart { get; set; }

    public int BitEnd { get; set; }

    public bool Required { get; set; }

    public bool Reserved { get; set; }

    /// <summary>args | master | leg | port | task</summary>
    public string Source { get; set; } = "";

    public int? Min { get; set; }

    public int? Max { get; set; }

    public Dictionary<string, string>? Enum { get; set; }
}

/// <summary>人工建单控件绑定。</summary>
public class OptionCodeInputDto
{
    public string Key { get; set; } = "";

    public string Label { get; set; } = "";

    public string Source { get; set; } = "";

    public bool Required { get; set; }

    public int? Min { get; set; }

    public int? Max { get; set; }

    public Dictionary<string, string>? Enum { get; set; }

    /// <summary>
    /// 写入 <see cref="CreateManualTaskDto"/> 的路径。
    /// args：<c>optionFields.{key}</c>；port：<c>fromPort</c>（Fetch）与 <c>toPort</c>（Put）；task：DTO 同名属性。
    /// </summary>
    public string Bind { get; set; } = "";

    /// <summary>仅 <c>source=port</c>：Fetch 腿绑定（<c>fromPort</c>）。</summary>
    public string? BindFetch { get; set; }

    /// <summary>仅 <c>source=port</c>：Put 腿绑定（<c>toPort</c>）。</summary>
    public string? BindPut { get; set; }
}
