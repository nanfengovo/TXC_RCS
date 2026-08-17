namespace TXC.RCS.Tasks.Enums;

/// <summary>
/// 任务来源。结束上报（RCS-101）等外部副作用只关心 <see cref="Mes"/>。
/// </summary>
public enum TaskSource
{
    /// <summary>人工 / 运营台建单。</summary>
    Manual = 0,

    /// <summary>MES RCS-001 派工。</summary>
    Mes = 1
}
