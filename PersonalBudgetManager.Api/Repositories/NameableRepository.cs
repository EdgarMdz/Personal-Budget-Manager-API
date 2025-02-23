using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class NameableRepository<T>(AppDbContext dbContext, IStrategy strategy)
        : Repository<T>(dbContext, strategy),
            INameableRepository<T>
        where T : class, IEntity, IHasNameColumn
    {
        private readonly DbSet<T> _nameableSet = dbContext.Set<T>();

        public async Task<T?> GetByNameAsync(string name, CancellationToken token)
        {
            async Task<T?> action(CancellationToken ct) =>
                await _nameableSet.FirstOrDefaultAsync(e => e.Name == name, ct);

            return await PerformDatabaseOperation(action, token);
        }
    }
}
