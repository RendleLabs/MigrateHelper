using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Polly;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper
{
    /// <summary>
    /// Helps with Migrations.
    /// </summary>
    public class MigrationHelper
    {
        private readonly ILogger<MigrationHelper> _logger;

        /// <summary>
        /// Constructs a new <see cref="MigrationHelper"/> with no logging.
        /// </summary>
        public MigrationHelper()
        {
            _logger = new NullLogger<MigrationHelper>();
        }

        /// <summary>
        /// Constructs a new <see cref="MigrationHelper"/> with logging to the given factory.
        /// </summary>
        /// <param name="loggerFactory">A <see cref="LoggerFactory"/> configured as required by your environment.</param>
        public MigrationHelper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MigrationHelper>();
        }

        /// <summary>
        /// Tries to connect to the database server and run the migration(s).
        /// </summary>
        /// <param name="args">Parameters from the Program <c>Main</c> method.</param>
        /// <returns></returns>
        public async Task TryMigrate(string[] args)
        {
            var context = Context(args) ?? new DesignTimeContextFactoryHelper(Assembly.GetEntryAssembly()).CreateContext(args);
            using (context)
            {
                await TryConnect(context);

                await TryRunMigration(context);
            }
        }

        public async Task TryMigrate<T>(string[] args, Func<T, Task> postMigrateCallback) where T : DbContext
        {
            if (postMigrateCallback == null) throw new ArgumentNullException(nameof(postMigrateCallback));

            T typedContext;

            try
            {
                typedContext = TryCreateTypedContext<T>(args);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.InvalidContextType, ex, ex.Message);
                throw;
            }
            
            using (typedContext)
            {
                await TryConnect(typedContext);

                if (await TryRunMigration(typedContext))
                {
                    // Only run the callback if the Migration actually ran, rather than failing with already run.
                    await TryRunCallback(typedContext, postMigrateCallback);
                }
            }
        }

        private T TryCreateTypedContext<T>(string[] args) where T : DbContext
        {
            var context = Context(args) ?? new DesignTimeContextFactoryHelper(Assembly.GetEntryAssembly()).CreateContext(args);
            if (!(context is T typedContext))
            {
                var message = $"Incorrect DbContext type. Expected {typeof(T).FullName} but was {context.GetType().FullName}.";
                throw new InvalidOperationException(message);
            }

            return typedContext;
        }

        private async Task TryRunCallback<T>(T context, Func<T, Task> callback)
        {
            try
            {
                await callback(context);
                _logger.LogInformation("Post-Migration callback ran successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.PostMigrateCallbackFailed, ex, "Post-migration callback failed: {message}", ex.Message);
                throw;
            }
        }

        private async Task<bool> TryRunMigration(DbContext context)
        {
            try
            {
                await context.Database.MigrateAsync();
                _logger.LogInformation("Migration ran successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(EventIds.MigrationFailed, ex, "Migration failed to run: {message}", ex.Message);
                return false;
            }
        }

        private async Task TryConnect(DbContext context)
        {
            try
            {
                await Policy
                    .Handle<Exception>(ex =>
                    {
                        _logger.LogWarning(EventIds.MigrationTestConnectFailed, ex, "TryMigrate test connect failed: '{message}'. Retrying...", ex.Message);
                        return true;
                    })
                    .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
                    .ExecuteAsync(async () =>
                    {
                        using (var connection = ServerConnection(context))
                        {
                            await connection.OpenAsync();
                            _logger.LogInformation("Connected: {connectionString}", connection.ConnectionString);
                        }
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.MigrationTestConnectFailed, ex, "TryMigrate could not connect to database.");
                throw;
            }
        }

        protected virtual DbConnection ServerConnection(DbContext context)
        {
            return new ConnectionHelper(context.Database.GetDbConnection()).WithoutDatabase();
        }

        protected virtual DbContext Context(string[] args)
        {
            return new DesignTimeContextFactoryHelper(Assembly.GetEntryAssembly()).CreateContext(args);
        }
    }
}