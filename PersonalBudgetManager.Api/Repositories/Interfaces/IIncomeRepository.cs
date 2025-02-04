using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IIncomeRepository : IRepository<Income>
    {
        public Task<IEnumerable<Income>> GetIncomesForUser(int userid, CancellationToken token);
    }
}
