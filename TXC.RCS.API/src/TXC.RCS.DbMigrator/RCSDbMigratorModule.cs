using TXC.RCS.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace TXC.RCS.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(RCSEntityFrameworkCoreModule),
    typeof(RCSApplicationContractsModule)
)]
public class RCSDbMigratorModule : AbpModule
{
}
