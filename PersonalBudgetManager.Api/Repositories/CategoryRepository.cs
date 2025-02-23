using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class CategoryRepository(AppDbContext dbContext, IStrategy strategy)
        : Repository<Category>(dbContext, strategy),
            ICategoryRepository
    {
        public async Task<Category?> FindUserCategory(
            int userId,
            string category,
            CancellationToken token
        )
        {
            async Task<Category?> action(CancellationToken ct) =>
                await _dbSet
                    .Where(c => c.UserId == userId && c.Name == category)
                    .FirstOrDefaultAsync(ct);

            return await PerformDatabaseOperation(action, token);
        }

        public async Task<IEnumerable<Category>> GetCategoriesForUser(
            int userId,
            CancellationToken token
        ) => await _dbSet.Where(c => c.UserId == userId).ToListAsync(token);
    }
}
