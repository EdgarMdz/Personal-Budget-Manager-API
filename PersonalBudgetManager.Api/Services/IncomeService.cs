using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class IncomeService(IUnitOfWork unitOfWork) : IIncomeService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IIncomeRepository _repo = unitOfWork.IncomeRepository;

        public async Task AddIncome(
            IncomeDTO incomeDTO,
            int categoryId,
            int userId,
            CancellationToken token
        )
        {
            IDbContextTransaction? transaction = null;
            try
            {
                Income newIncome = new()
                {
                    Description = incomeDTO.Description,
                    Amount = incomeDTO.Amount,
                    CategoryId = categoryId,
                    UserId = userId,
                };

                transaction = await _unitOfWork.BeginTransactionAsync(token);
                await _repo.InsertAsync(newIncome, token);
                await _unitOfWork.SaveChangesAsync(token);
                await _unitOfWork.CommitTransactionAsync(token);
            }
            catch (Exception)
            {
                if (transaction != null)
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);

                throw;
            }
        }

        public async Task<IEnumerable<IncomeDTO>> GetIncomes(int userId, CancellationToken token)
        {
            try
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
                            Category = income.Category?.Name ?? "",
                        }
                    );
                }

                return incomes;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
