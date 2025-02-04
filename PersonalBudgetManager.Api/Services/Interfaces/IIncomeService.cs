using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IIncomeService
    {
        Task<IncomeDTO> AddIncome(
            IncomeDTO incomeDTO,
            int categoryId,
            int userId,
            CancellationToken token
        );
        public Task<IncomeDTO> GetIncomeById(int incomeId, int userId, CancellationToken token);
        public Task<IEnumerable<IncomeDTO>> GetIncomes(int userId, CancellationToken token);
        public Task<IncomeDTO> UpdateIncome(
            IncomeDTO incomeDTO,
            int userId,
            CancellationToken token
        );
    }
}
