using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.Locations;

public class NoOpErackGate : IErackGate, ITransientDependency
{
    public Task EnsureReadyAsync(TaskCreateErackRequest request, CancellationToken ct = default)
        => Task.CompletedTask;
}
