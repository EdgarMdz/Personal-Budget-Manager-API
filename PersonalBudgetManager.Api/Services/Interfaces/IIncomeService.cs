using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IIncomeService
    {
        Task AddIncome(IncomeDTO income, int categoryId, int userId, CancellationToken token);
        public Task<IEnumerable<IncomeDTO>> GetIncomes(int userId, CancellationToken token);
    }
}
