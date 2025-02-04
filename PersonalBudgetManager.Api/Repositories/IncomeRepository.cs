using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class IncomeRepository(AppDbContext dbContext)
        : Repository<Income>(dbContext),
            IIncomeRepository
    {
        public async Task<IEnumerable<Income>> GetIncomesForUser(
            int userid,
            CancellationToken token
        ) =>
            await PerformDatabaseOperation(async () =>
            {
                return await _dbSet
                    .Where(income => income.UserId == userid)
                    .OrderByDescending(income => income.Date)
                    .ToListAsync(token);
            });
    }
}
