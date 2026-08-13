using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TXC.RCS.Data;
using Volo.Abp.DependencyInjection;

namespace TXC.RCS.EntityFrameworkCore;

public class EntityFrameworkCoreRCSDbSchemaMigrator
    : IRCSDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreRCSDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the RCSDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<RCSDbContext>()
            .Database
            .MigrateAsync();
    }
}
