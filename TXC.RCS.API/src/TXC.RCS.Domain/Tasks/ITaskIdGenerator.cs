using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace TXC.RCS.Tasks
{
    public interface ITaskIdGenerator
    {
        Task<string> NextAsync(CancellationToken ct = default);
    }
}