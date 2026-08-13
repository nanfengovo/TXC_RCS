using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.EventConst
{
    public static class TaskEvents
    {
        /// <summary>
        /// 任务开始
        /// </summary>
        public const string TaskStarted = "TaskStarted";         // task_info

        /// <summary>
        /// 到达目标
        /// </summary>  
        public const string Arrived = "Arrived";                 // task_arrive_target

        /// <summary>
        /// 请求许可
        /// </summary>
        public const string PermitRequested = "PermitRequested"; // robot_permiss_start_action

        /// <summary>
        /// 完成
        /// </summary>
        public const string Finished = "Finished";               // task_finish

        /// <summary>
        /// 删除请求
        /// </summary>
        public const string DeleteRequested = "DeleteRequested"; // TM 网页申请删

        /// <summary>
        /// RFID检查
        /// </summary>
        public const string RfidCheck = "RfidCheck";             // 可选
    }
}