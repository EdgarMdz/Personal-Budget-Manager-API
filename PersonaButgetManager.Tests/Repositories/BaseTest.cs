using PersonaButgetManager.Tests.Common.Data;
using PersonaButgetManager.Tests.Common.Entities;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonaButgetManager.Tests.Repositories
{
    public class BaseTest : IDisposable
    {
        protected TestDBContext _dbcontext;

        public BaseTest()
        {
            _dbcontext = new TestDBContext();
        }

        public void Dispose()
        {
            _dbcontext.Dispose();
            GC.SuppressFinalize(this);
        }

        protected async Task<IEnumerable<T>> ResetDb<T>(int numberOfEntities)
            where T : class
        {
            ArgumentOutOfRangeException.ThrowIfNegative(numberOfEntities, nameof(numberOfEntities));

            await _dbcontext.Database.EnsureDeletedAsync();
            await _dbcontext.Database.EnsureCreatedAsync();
            IEnumerable<T> entities = typeof(T).Name switch
            {
                nameof(TestEntity) => TestEntityRecordsFactory.CreateTestEntities(numberOfEntities)
                    as IEnumerable<T>
                    ?? [],
                nameof(Category) => TestEntityRecordsFactory.CreateCategories(numberOfEntities)
                    as IEnumerable<T>
                    ?? [],
                _ => throw new ArgumentException("Invalid entity type"),
            };
            ;

            await _dbcontext.Set<T>().AddRangeAsync(entities);
            await _dbcontext.SaveChangesAsync();

            return entities;
        }
    }

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
    }
}
