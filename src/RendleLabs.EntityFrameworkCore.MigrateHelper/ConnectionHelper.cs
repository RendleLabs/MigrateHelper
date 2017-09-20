using System;
using System.Data.Common;
using System.Linq;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper
{
    internal class ConnectionHelper
    {
        private readonly string _connectionStringWithoutDatabase;
        private readonly Type _connectionType;
        public ConnectionHelper(DbConnection connection)
        {
            _connectionType = connection.GetType();
            _connectionStringWithoutDatabase = RemoveDatabaseFromString(connection);
        }

        public DbConnection WithoutDatabase()
        {
            return Activator.CreateInstance(_connectionType, _connectionStringWithoutDatabase) as DbConnection;
        }

        public static string RemoveDatabaseFromString(DbConnection connection)
        {
            var builderType = ConnectionStringBuilderType(connection);
            if (builderType == null) return null;
            if (!(Activator.CreateInstance(builderType, connection.ConnectionString) is DbConnectionStringBuilder builder)) return null;
            builder.Remove("Database");
            return builder.ConnectionString;
        }

        public static Type ConnectionStringBuilderType(DbConnection connection)
        {
            var types = connection.GetType().Assembly.ExportedTypes
                .Where(typeof(DbConnectionStringBuilder).IsAssignableFrom).ToArray();
            switch (types.Length)
            {
                case 0:
                    return null;
                case 1:
                    return types[0];
                default:
                    return types.FirstOrDefault(t => t.Name.StartsWith(connection.GetType().Name));
            }
        }
    }
}