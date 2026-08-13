using System.Threading;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.TM;

/// <summary>Sim：不访问真 TM，直接成功。由 Host 按 Mode 注册为 ITmClient。</summary>
public class SimulationTmClient : ITmClient
{
    public Task TaskAddAsync(TmTaskAddRequest request, CancellationToken ct = default)
        => Task.CompletedTask;
}
