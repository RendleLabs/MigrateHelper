using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole();
            await new MigrationHelper(loggerFactory).TryMigrate<FooContext>(args, async context =>
            {
                context.Foos.Add(new Foo {Name = "Test"});
                await context.SaveChangesAsync();
            });
        }
    }
}
