using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IExpensesRepository : IRepository<Expense>
    {
        public Task<IEnumerable<Expense>> GetExpensesForUser(int userid, CancellationToken token);
    }
}
