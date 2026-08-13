using Volo.Abp.Modularity;

namespace TXC.RCS;

[DependsOn(
    typeof(RCSApplicationModule),
    typeof(RCSDomainTestModule)
)]
public class RCSApplicationTestModule : AbpModule
{

}
