using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class CategoryRepository(AppDbContext dbContext, IDelayProvider delayProvider)
        : Repository<Category>(dbContext, delayProvider),
            ICategoryRepository
    {
        public async Task<Category?> FindUserCategory(
            int userId,
            string category,
            CancellationToken token
        ) =>
            await PerformDatabaseOperation(
                async () =>
                    await _dbSet
                        .Where(c => c.UserId == userId && c.Name == category)
                        .FirstOrDefaultAsync(token)
            );

        public async Task<IEnumerable<Category>> GetCategoriesForUser(
            int userId,
            CancellationToken token
        ) => await _dbSet.Where(c => c.UserId == userId).ToListAsync(token);
    }
}
