using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class IncomeService(IUnitOfWork unitOfWork) : BaseService(unitOfWork), IIncomeService
    {
        private readonly IIncomeRepository _repo = unitOfWork.IncomeRepository;

        public async Task<IncomeDTO> AddIncome(
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
                newIncome = await _repo.InsertAsync(newIncome, token);

                if (newIncome == null)
                    throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                Category? category = newIncome.CategoryId.HasValue
                    ? await _unitOfWork.CategoryRepository.GetByIdAsync(
                        newIncome.CategoryId.Value,
                        token
                    )
                    : null;

                IncomeDTO income = new()
                {
                    Id = newIncome.Id,
                    Description = newIncome.Description,
                    Amount = newIncome.Amount,
                    Date = newIncome.Date,
                    Category = category?.Name ?? string.Empty,
                };

                return income;
            }
            catch (Exception)
            {
                if (transaction != null)
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);

                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }
        }

        public async Task<IncomeDTO> DeleteIncome(int incomeId, int userId, CancellationToken token)
        {
            async Task<IncomeDTO> action()
            {
                if (
                    await _repo.GetByIdAsync(incomeId, token) is not Income income
                    || income.UserId != userId
                )
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                income =
                    await _repo.DeleteAsync(incomeId, token)
                    ?? throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                return new()
                {
                    Id = income.Id,
                    Category = income.Category?.Name ?? string.Empty,
                    Description = income.Description,
                    Amount = income.Amount,
                    Date = income.Date,
                };
            }

            return await PerformTransactionalOperation(action, token);
        }

        public async Task<IncomeDTO> GetIncomeById(
            int incomeId,
            int userId,
            CancellationToken token
        )
        {
            async Task<IncomeDTO> action()
            {
                if (await _repo.GetByIdAsync(incomeId, token) is not Income income)
                    throw new InvalidOperationException(ErrorMessages.EntryNotFound);

                if (income.UserId != userId)
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                IncomeDTO incomeDTO = new()
                {
                    Id = income.Id,
                    Amount = income.Amount,
                    Description = income.Description,
                    Date = income.Date,
                    Category = income.Category?.Name ?? string.Empty,
                };

                return incomeDTO;
            }

            return await PerformTransactionalOperation(action, token);
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
                            Id = income.Id,
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

        public async Task<IncomeDTO> UpdateIncome(
            IncomeDTO incomeDTO,
            int userId,
            CancellationToken token
        )
        {
            async Task<IncomeDTO> action()
            {
                if (incomeDTO.Id == null)
                    throw new InvalidOperationException(
                        $"{ErrorMessages.ProvideParater}: {nameof(incomeDTO.Id)}"
                    );

                if (
                    await _repo.GetByIdAsync(incomeDTO.Id.Value, token) is not Income existingIncome
                    || existingIncome.UserId != userId
                )
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                Category? category = await GetCategory(incomeDTO.Category, userId, token);

                existingIncome.Id = incomeDTO.Id.Value;
                existingIncome.Description = incomeDTO.Description;
                existingIncome.Date = incomeDTO.Date;
                existingIncome.Amount = incomeDTO.Amount;
                existingIncome.CategoryId =
                    category?.Id
                    ?? throw new InvalidOperationException(ErrorMessages.NotRegisteredCategory);

                if (await _repo.UpdateAsync(existingIncome, token) is not Income updatedIncome)
                    throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                return new()
                {
                    Id = updatedIncome.Id,
                    Description = updatedIncome.Description,
                    Amount = updatedIncome.Amount,
                    Date = updatedIncome.Date,
                    Category = category != null ? category.Name : string.Empty,
                };
            }

            return await PerformTransactionalOperation(action, token);
        }

        private async Task<Category?> GetCategory(
            string category,
            int userId,
            CancellationToken token
        ) => await _unitOfWork.CategoryRepository.FindUserCategory(userId, category, token);
    }
}
