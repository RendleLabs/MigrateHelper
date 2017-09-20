using System.Threading.Tasks;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new MigrationHelper().TryMigrate(args);
        }
    }
}
