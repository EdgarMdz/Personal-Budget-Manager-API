using PersonaButgetManager.Tests.Common.Entities;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonaButgetManager.Tests.Common.Factories
{
    public class TestEntityRecordsFactory
    {
        public static IEnumerable<TestEntity> CreateTestEntities(int numberOfEntities) =>
            Enumerable
                .Range(1, numberOfEntities)
                .Select(i => new TestEntity() { Name = $"Entity {i}", Id = i });

        public static IEnumerable<Category> CreateCategories(int numberOfCategories) =>
            Enumerable
                .Range(1, numberOfCategories)
                .Select(i => new Category()
                {
                    Name = $"Category {i}",
                    Id = i,
                    UserId = i,
                });

        public static IEnumerable<Expense> CreateExpenses(int numberOfCategories) =>
            Enumerable
                .Range(1, numberOfCategories)
                .Select(i => new Expense()
                {
                    Id = i,
                    UserId = i,
                    Date = DateTime.UtcNow,
                    Amount = i,
                    Description = $"Expense {i}",
                    CategoryId = i,
                });
    }
}
