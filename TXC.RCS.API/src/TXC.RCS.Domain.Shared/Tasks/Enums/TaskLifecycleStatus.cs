namespace TXC.RCS.Tasks.Enums
{
    public enum TaskLifecycleStatus
    {
        /// <summary>
        /// 待开始
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 运行中
        /// </summary>
        Running = 1,

        /// <summary>
        /// 成功
        /// </summary>
        Succeeded = 2,

        /// <summary>
        /// 失败
        /// </summary>
        Failed = 3,

        /// <summary>
        /// 取消
        /// </summary>

        Canceled = 4
    }
}