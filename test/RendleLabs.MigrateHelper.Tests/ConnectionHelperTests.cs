using System.Data.SqlClient;
using Npgsql;
using Xunit;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.Tests
{
    public class ConnectionHelperTests
    {
        [Fact]
        public void GetsNpgsqlConnectionStringBuilderType()
        {
            var actual =
                ConnectionHelper.ConnectionStringBuilderType(
                    new NpgsqlConnection("Host=foo;Database=bar;Username=wibble;Password=quux"));
            Assert.Equal(typeof(NpgsqlConnectionStringBuilder), actual);
        }

        [Fact]
        public void RemovesNpgsqlDatabase()
        {
            var actual =
                ConnectionHelper.RemoveDatabaseFromString(
                    new NpgsqlConnection("Host=foo;Database=bar;Username=wibble;Password=quux"));
            Assert.Equal("Host=foo;Username=wibble;Password=quux", actual);
        }

        [Fact]
        public void CreatesNpgsqlConnection()
        {
            var target = new ConnectionHelper(new NpgsqlConnection("Host=foo;Database=bar;Username=wibble;Password=quux"));
            var actual = target.WithoutDatabase();
            Assert.IsType<NpgsqlConnection>(actual);
        }

        [Fact]
        public void GetsSqlConnectionStringBuilderType()
        {
            var actual =
                ConnectionHelper.ConnectionStringBuilderType(
                    new SqlConnection("Data Source=foo;initial catalog=bar;user id=wibble;password=quux"));
            Assert.Equal(typeof(SqlConnectionStringBuilder), actual);
        }

        [Fact]
        public void RemovesSqlDatabase()
        {
            var actual =
                ConnectionHelper.RemoveDatabaseFromString(
                    new SqlConnection("Data Source=foo;initial catalog=bar;user id=wibble;password=quux"));
            Assert.Equal("Data Source=foo;User ID=wibble;Password=quux", actual);
        }

        [Fact]
        public void CreatesSqlConnection()
        {
            var target = new ConnectionHelper(new SqlConnection("Data Source=foo;initial catalog=bar;user id=wibble;password=quux"));
            var actual = target.WithoutDatabase();
            Assert.IsType<SqlConnection>(actual);
        }
    }
}