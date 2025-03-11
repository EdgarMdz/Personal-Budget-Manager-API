using Microsoft.EntityFrameworkCore;
using PersonaButgetManager.Tests.Common.Entities;
using PersonalBudgetManager.Api.DataContext;

namespace PersonaButgetManager.Tests.Common.Data
{
    public class TestDBContext : AppDbContext
    {
        public TestDBContext()
            : base(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            ) { }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
}
