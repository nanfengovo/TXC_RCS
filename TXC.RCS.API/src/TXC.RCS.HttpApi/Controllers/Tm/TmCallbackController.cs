using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Microsoft.AspNetCore.Authorization;
using TXC.RCS.Swagger;
using TXC.RCS.Tasks.Tm;
using TXC.RCS.Tasks.EventConst;

namespace TXC.RCS.Controllers.Tm
{
    [RemoteService]
    [Route("api/v1/xinsong")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = RcsSwaggerDocs.Biz)]
    public class TmCallbackController : RCSController
    {
        private readonly ITmCallbackAppService _callbacks;

        public TmCallbackController(ITmCallbackAppService callbacks)
        {
            _callbacks = callbacks;
        }

        /// <summary>
        /// 任务开始 对应领域（TaskStarted）
        /// </summary>
        [HttpPost("task_info")]
        public Task<TmCallbackHttpResponse> TaskInfoAsync([FromBody] TmCallbackRequestDto body)
        => _callbacks.HandleAsync(TaskEvents.TaskStarted,body);

        /// <summary>
        /// 到达目标点 对应领域（Arrived）
        /// </summary>
        [HttpPost("task_arrive_target")]
        public Task<TmCallbackHttpResponse> ArriveAsync([FromBody] TmCallbackRequestDto body)
        => _callbacks.HandleAsync(TaskEvents.Arrived,body);

        /// <summary>
        /// 请求放行 对应领域（PermitRequested）
        /// </summary>
        [HttpPost("robot_permiss_start_action")]
        public Task<TmCallbackHttpResponse> PermitAsync([FromBody] TmCallbackRequestDto body)
        => _callbacks.HandleAsync(TaskEvents.PermitRequested,body);

        /// <summary>
        /// 子任务完成 对应领域（Finished）
        /// </summary>
        [HttpPost("task_finish")]
        public Task<TmCallbackHttpResponse> FinishAsync([FromBody] TmCallbackRequestDto body)
        => _callbacks.HandleAsync(TaskEvents.Finished,body);
    }
}