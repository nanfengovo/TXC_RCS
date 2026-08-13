using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.EventConst
{
    public class WorkflowActivities
    {
        /// <summary>
        /// TM 排发
        /// </summary>
        public const string TmDispatch = "Tm.Dispatch";

        /// <summary>
        /// TM 回复许可
        /// </summary>
        public const string TmReplyPermit = "Tm.ReplyPermit";

        /// <summary>
        /// TM 删除
        /// </summary>
        public const string TmDelete = "Tm.Delete";

        /// <summary>
        /// 执行完成
        /// </summary>
        public const string ExecutionComplete = "Execution.Complete";
    }
}