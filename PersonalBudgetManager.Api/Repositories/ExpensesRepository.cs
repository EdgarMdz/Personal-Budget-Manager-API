using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class ExpensesRepository(AppDbContext context, IDelayProvider delayProvider)
        : Repository<Expense>(context, delayProvider),
            IExpensesRepository
    {
        public async Task<IEnumerable<Expense>> GetExpensesForUser(
            int userid,
            CancellationToken token
        ) =>
            await PerformDatabaseOperation(
                async () =>
                    await _dbSet
                        .Where(e => e.UserId == userid)
                        .OrderByDescending(e => e.Date)
                        .ToListAsync(token)
            );
    }
}
