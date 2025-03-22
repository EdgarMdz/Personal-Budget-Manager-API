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
        {
            ArgumentOutOfRangeException.ThrowIfNegative(numberOfEntities, nameof(numberOfEntities));

            await _dbcontext.Database.EnsureDeletedAsync();
            await _dbcontext.Database.EnsureCreatedAsync();

            IEnumerable<T> entities;

            switch (typeof(T).Name)
            {
                case nameof(TestEntity):
                    entities =
                        TestEntityRecordsFactory.CreateTestEntities(numberOfEntities)
                            as IEnumerable<T>
                        ?? [];
                    await _dbcontext.TestEntities.AddRangeAsync(entities.Cast<TestEntity>());
                    break;
                case nameof(Category):
                    entities =
                        TestEntityRecordsFactory.CreateCategories(numberOfEntities)
                            as IEnumerable<T>
                        ?? [];
                    await _dbcontext.Categories.AddRangeAsync(entities.Cast<Category>());
                    break;
                default:
                    throw new ArgumentException("Invalid entity type");
            }
            ;

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
