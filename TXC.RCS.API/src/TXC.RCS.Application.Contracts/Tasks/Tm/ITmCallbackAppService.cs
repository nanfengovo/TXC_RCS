using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace TXC.RCS.Tasks.Tm
{
    /// <summary>
    /// TM 回调应用服务。
    /// </summary>
    public interface ITmCallbackAppService : IApplicationService
    {
        Task<TmCallbackHttpResponse> HandleAsync(string eventName,TmCallbackRequestDto input);
    }
}