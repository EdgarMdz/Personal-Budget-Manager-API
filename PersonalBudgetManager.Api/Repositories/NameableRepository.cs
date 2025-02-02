using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class NameableRepository<T>(AppDbContext dbContext)
        : Repository<T>(dbContext),
            INameableRepository<T>
        where T : class, IEntity, IHasNameColumn
    {
        private readonly DbSet<T> _nameableSet = dbContext.Set<T>();

        public async Task<T?> GetByNameAsync(string name, CancellationToken token) =>
            await PerformDatabaseOperation(
                async () => await _nameableSet.FirstOrDefaultAsync(e => e.Name == name)
            );
    }
}
