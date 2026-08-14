using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Tm
{
    public class TmCallbackRequestDto
    {
        /// <summary>
        /// TM子任务流水号，RCS用来反查TM任务。、
        /// </summary>
        [JsonPropertyName("task_serial")]
        public string? TaskSerial { get; set;}

        /// <summary>
        /// AGV 编号；可选，有则写入 TaskDo.AgvSerial
        /// </summary>
        [JsonPropertyName("AGV_serial")]
        public string? AgvSerial { get; set;}

        /// <summary>
        /// 目标地址；可选，上报车辆到达目的地时才会用到
        /// </summary>
        [JsonPropertyName("target")]
        public string? Target {get; set;}


        /// <summary>
        /// 选项码；可选，请求设备动作放行
        /// </summary>
        [JsonPropertyName("option_code")]
        public string? OptionCode {get; set;}

    }
}