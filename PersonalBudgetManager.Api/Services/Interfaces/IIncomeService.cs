using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IIncomeService
    {
        public Task<IEnumerable<IncomeDTO>> GetIncomes(int userId, CancellationToken token);
    }
}
