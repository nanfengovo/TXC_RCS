namespace TXC.RCS.Tasks.Mes;

/// <summary>RCS-101 job_result 取值（文档约定字符串）。</summary>
public static class MesJobResults
{
    /// <summary>执行完成。</summary>
    public const string Completed = "1";

    /// <summary>任务删除 / 取消。</summary>
    public const string Deleted = "2";
}

/// <summary>MES 回包 job_status。</summary>
public static class MesJobStatuses
{
    public const string Rejected = "0";
    public const string Accepted = "1";
}
