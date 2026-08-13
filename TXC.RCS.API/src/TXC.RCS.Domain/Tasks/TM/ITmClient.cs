using System.Threading;
using System.Threading.Tasks;

namespace TXC.RCS.Tasks.TM;

public interface ITmClient
{
    Task TaskAddAsync(TmTaskAddRequest request, CancellationToken ct = default);
}
