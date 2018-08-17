# MigrateHelper
Migration helper for creating console apps to run migrations in EF core

## Installation

NuGet: [RendleLabs.EntityFrameworkCore.MigrateHelper](https://www.nuget.org/packages/RendleLabs.EntityFrameworkCore.MigrateHelper/)

```
dotnet add package RendleLabs.EntityFrameworkCore.MigrateHelper
```

## Usage

To use MigrateHelper, create a Console application and make sure it contains a single implementation of [IDesignTimeDbContactFactory&lt;T&gt;](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.design.idesigntimedbcontextfactory-1?view=efcore-2.1) that creates your DB context.

Something like this:

```csharp
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
```

Then, in your `Program.cs` file, call the MigrationHelper like this:

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddConsole();
        await new MigrationHelper(loggerFactory).TryMigrate(args);
    }
}
```

You can obviously add any Loggers you like to get output.

If you want to run some addtional code after the migration, you can add a callback
like this:

```csharp
    await new MigrationHelper(loggerFactory).TryMigrate<FooContext>(args, async context =>
    {
        context.Foos.Add(new Foo {Name = "Test"});
        await context.SaveChangesAsync();
    });
```

This code will only be run if the migration itself runs successfully. If the migration
has already been run, this code will not be called.

You can now build and run your console app, passing the connection string as a
command line argument. This makes it easy to run migrations in Docker or
Kubernetes using a Secret as the argument.