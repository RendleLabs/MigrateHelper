using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.TestConsole
{
    public class DesignTimeFooContextFactory : IDesignTimeDbContextFactory<FooContext>
    {
        public const string LocalPostgres = "Host=localhost;Database=mighelp;Username=mig;Password=secretsquirrel";
        public static readonly string MigrationAssemblyName =
            typeof(DesignTimeFooContextFactory).Assembly.GetName().Name;


        public FooContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<FooContext>()
                .UseNpgsql(args.FirstOrDefault() ?? LocalPostgres, b => b.MigrationsAssembly(MigrationAssemblyName));
            return new FooContext(builder.Options);
        }
    }
}