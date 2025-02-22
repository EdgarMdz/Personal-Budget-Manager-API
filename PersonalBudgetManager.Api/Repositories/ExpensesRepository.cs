using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class ExpensesRepository(AppDbContext context)
        : Repository<Expense>(context),
            IExpensesRepository
    {
        public async Task<IEnumerable<Expense>> GetExpensesForUser(
            int userid,
            CancellationToken token
        )
        {
            async Task<IEnumerable<Expense>> action(CancellationToken ct) =>
                await _dbSet
                    .Where(e => e.UserId == userid)
                    .OrderByDescending(e => e.Date)
                    .ToListAsync(ct);

            return await PerformDatabaseOperation(action, token);
        }
    }
}
