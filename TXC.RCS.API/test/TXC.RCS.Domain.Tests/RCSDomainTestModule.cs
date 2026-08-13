using Volo.Abp.Modularity;

namespace TXC.RCS;

[DependsOn(
    typeof(RCSDomainModule),
    typeof(RCSTestBaseModule)
)]
public class RCSDomainTestModule : AbpModule
{

}
