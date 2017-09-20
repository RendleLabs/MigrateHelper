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
        public async Task TryMigrate(string[] args, params (string key,string value)[] tags)
        {
            var context = Context(args) ?? new DesignTimeContextFactoryHelper(Assembly.GetEntryAssembly()).CreateContext(args);
            using (context)
            {
                await TryConnect(context);

                await TryRunMigration(context);
            }
        }

        private async Task TryRunMigration(DbContext context)
        {
            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                // Ignored
                _logger.LogError(EventIds.MigrationFailed, ex, "Migration failed to run: {message}", ex.Message);
            }
        }

        private async Task TryConnect(DbContext context)
        {
            try
            {
                await Policy
                    .Handle<DbException>(ex =>
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
