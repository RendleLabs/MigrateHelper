using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

[assembly:InternalsVisibleTo("RendleLabs.EntityFrameworkCore.MigrateHelper.Tests")]

namespace RendleLabs.EntityFrameworkCore.MigrateHelper
{
    internal class DesignTimeContextFactoryHelper
    {
        private readonly Assembly _assembly;

        public DesignTimeContextFactoryHelper(Assembly assembly)
        {
            _assembly = assembly;
        }

        public Type GetFactoryType()
        {
            return (from type in _assembly.DefinedTypes
                from implementedInterface in type.ImplementedInterfaces
                where implementedInterface.IsGenericType
                where implementedInterface.GetGenericTypeDefinition() == typeof(IDesignTimeDbContextFactory<>)
                select type).FirstOrDefault();
        }

        public DbContext CreateContext(string[] args)
        {
            var type = GetFactoryType();
            var factory = Activator.CreateInstance(type);
            var method =
                type.GetMethod(nameof(IDesignTimeDbContextFactory<DbContext>
                    .CreateDbContext));
            var objArgs = new object[] {args};
            return method.Invoke(factory, objArgs) as DbContext;
        }
    }
}