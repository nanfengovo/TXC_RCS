using Volo.Abp.Modularity;

namespace TXC.RCS;

public abstract class RCSApplicationTestBase<TStartupModule> : RCSTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
