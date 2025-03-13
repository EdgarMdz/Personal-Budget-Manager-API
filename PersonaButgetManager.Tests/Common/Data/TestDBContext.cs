using Microsoft.EntityFrameworkCore;
using PersonaButgetManager.Tests.Common.Entities;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;

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
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
