using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class IncomeRepository(AppDbContext dbContext, IStrategy strategy)
        : Repository<Income>(dbContext, strategy),
            IIncomeRepository
    {
        public async Task<IEnumerable<Income>> GetIncomesForUser(
            int userid,
            CancellationToken token
        )
        {
            async Task<IEnumerable<Income>> action(CancellationToken ct) =>
                await _dbSet
                    .Where(income => income.UserId == userid)
                    .OrderByDescending(income => income.Date)
                    .ToListAsync(ct);

            return await PerformDatabaseOperation(action, token);
        }
    }
}
