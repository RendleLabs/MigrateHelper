using System.ComponentModel.DataAnnotations;

namespace RendleLabs.EntityFrameworkCore.MigrateHelper.TestConsole
{
    public class Foo
    {
        public int Id { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
    }
}