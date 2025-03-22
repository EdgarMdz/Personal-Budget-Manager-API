using PersonaButgetManager.Tests.Common.Data;
using PersonaButgetManager.Tests.Common.Entities;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonaButgetManager.Tests.Repositories
{
    public class BaseTest : IDisposable
    {
        protected TestDBContext _dbcontext;

        private static readonly Dictionary<Type, Func<int, IEnumerable<object>>> EntityFactory =
            new()
            {
                { typeof(TestEntity), TestEntityRecordsFactory.CreateTestEntities },
                { typeof(Category), TestEntityRecordsFactory.CreateCategories },
            };

        public BaseTest() => _dbcontext = new TestDBContext();

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

            if (!EntityFactory.TryGetValue(typeof(T), out var factory))
                throw new ArgumentException($"There is not factory defined for {typeof(T).Name}");

            var entities = factory(numberOfEntities).Cast<T>();

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
