using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class IncomeService(IUnitOfWork unitOfWork) : IIncomeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IIncomeRepository _repo = unitOfWork.IncomeRepository;

        public async Task<IEnumerable<IncomeDTO>> GetIncomes(int userId, CancellationToken token)
        {
            var userIncomes = await _repo.GetIncomesForUser(userId, token);

            List<IncomeDTO> incomes = [];

            foreach (Income income in userIncomes)
            {
                incomes.Add(
                    new()
                    {
                        Description = income.Description,
                        Date = income.Date,
                        Amount = income.Amount,
                        Category = income.Category?.Name,
                    }
                );
            }

            return incomes;
        }
    }
}
