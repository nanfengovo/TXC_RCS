using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Polly.Caching;
using Volo.Abp.DependencyInjection;
using System.Threading;

namespace TXC.RCS.Tasks
{
    public class TimestampTaskIdGenerator : ITaskIdGenerator,ISingletonDependency
    {
        private readonly object _lock = new object();
        private string _daySecond = "";
        private int _seq;

        public Task<string> NextAsync(CancellationToken ct = default)
        {
            var stamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            lock (_lock)
            {
                if(_daySecond != stamp)
                {
                    _daySecond = stamp;
                    _seq = 0;
                }
                else
                {
                    _seq++;
                }
                return Task.FromResult($"{stamp}{_seq:D6}");
            }
        }
    }
}