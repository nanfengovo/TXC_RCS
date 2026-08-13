using System.Threading.Tasks;

namespace TXC.RCS.Data;

public interface IRCSDbSchemaMigrator
{
    Task MigrateAsync();
}
