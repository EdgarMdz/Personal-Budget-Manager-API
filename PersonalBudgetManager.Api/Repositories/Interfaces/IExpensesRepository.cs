using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IExpensesRepository
    {
        public Task<IEnumerable<Expense>> GetExpencesForUser(int userid, CancellationToken token);
    }
}
