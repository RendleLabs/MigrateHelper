using Microsoft.EntityFrameworkCore;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.TestConsole
{
    public class FooContext : DbContext
    {
        public FooContext(DbContextOptions options) : base(options)
        {
        }

        public FooContext()
        {
        }

        public DbSet<Foo> Foos { get; set; }
    }
}