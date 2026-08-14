using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.Tm
{
    public class TmCallbackHttpResponse
    {
        [JsonPropertyName("Result")]
        public bool Result {get; set;}

        [JsonPropertyName("ErrMsg")]
        public string? ErrMsg {get; set;}

        /// <summary>
        /// 请求放行时为option_code和task_serial 物料 RFID 校验时为rfid_result，其他情况为空
        /// </summary>
        [JsonPropertyName("data")]
        public Dictionary<string,string>? Data {get; set;}


        public static TmCallbackHttpResponse Ok(IReadOnlyDictionary<string,string>? data = null) 
            => new()
            {
                Result = true,
                ErrMsg = "",
                Data = data is {Count: > 0} ? new Dictionary<string,string>(data) : null
            };

        public static TmCallbackHttpResponse Fail(string errMsg)
            => new() { Result = false, ErrMsg = errMsg };

    }
}