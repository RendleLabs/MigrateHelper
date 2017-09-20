using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using Xunit;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.Tests
{
    public class DesignTimeContextFactoryHelperTests
    {
        [Fact]
        public void GetsFactoryTypeFromThisAssembly()
        {
            var target = new DesignTimeContextFactoryHelper(Assembly.GetExecutingAssembly());
            var actual = target.GetFactoryType();
            Assert.Equal(typeof(TestFactoryType), actual);
        }

        [Fact]
        public void CreatesContext()
        {
            var target = new DesignTimeContextFactoryHelper(Assembly.GetExecutingAssembly());
            var actual = target.CreateContext(new string[0]);
            Assert.IsType<TestContext>(actual);
        }

        [Fact]
        public void SqlStringHacking()
        {
            DbConnectionStringBuilder cn = new SqlConnectionStringBuilder("Data Source=foo;initial catalog=bar;user id=wibble;password=quux");
            cn.Remove("Database");
            Assert.Equal("Data Source=foo;User ID=wibble;Password=quux", cn.ConnectionString);
        }

        [Fact]
        public void NpgsqlStringHacking()
        {
            DbConnectionStringBuilder cn = new NpgsqlConnectionStringBuilder("Host=foo;Database=bar;Username=wibble;Password=quux");
            cn.Remove("Database");
            Assert.Equal("Host=foo;Username=wibble;Password=quux", cn.ConnectionString);
        }
    }

    public class TestFactoryType : IDesignTimeDbContextFactory<TestContext>
    {
        public TestContext CreateDbContext(string[] args)
        {
            return new TestContext();
        }
    }

    public class TestContext : DbContext
    {
        
    }
}
