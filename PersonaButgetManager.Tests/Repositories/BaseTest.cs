using PersonaButgetManager.Tests.Common.Data;
using PersonaButgetManager.Tests.Common.Entities;

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

        protected async Task<IEnumerable<TestEntity>> ResetDb(int numberOfEntities)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(numberOfEntities, nameof(numberOfEntities));

            await _dbcontext.Database.EnsureDeletedAsync();
            await _dbcontext.Database.EnsureCreatedAsync();

            var entities = Enumerable
                .Range(1, numberOfEntities)
                .Select(i => new TestEntity() { Name = $"Entity {i}", Id = i });

            await _dbcontext.AddRangeAsync(entities);
            await _dbcontext.SaveChangesAsync();

            return entities;
        }
    }
}
