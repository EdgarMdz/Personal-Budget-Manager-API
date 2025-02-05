using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class ExpensesService(IUnitOfWork unitOfWork) : BaseService(unitOfWork), IExpensesService
    {
        private readonly IExpensesRepository _repo = unitOfWork.ExpensesRepository;

        public async Task<ExpenseDTO> AddExpense(
            ExpenseDTO expenseDTO,
            int categoryId,
            int userId,
            CancellationToken token
        )
        {
            async Task<ExpenseDTO> action()
            {
                Expense newExpense = new()
                {
                    Description = expenseDTO.Description,
                    Amount = expenseDTO.Amount,
                    CategoryId = categoryId,
                    UserId = userId,
                };

                newExpense = await _repo.InsertAsync(newExpense, token);

                if (newExpense == null)
                    throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                Category? category = newExpense.CategoryId.HasValue
                    ? await _unitOfWork.CategoryRepository.GetByIdAsync(
                        newExpense.CategoryId.Value,
                        token
                    )
                    : null;

                ExpenseDTO expense = new()
                {
                    Id = newExpense.Id,
                    Description = newExpense.Description,
                    Amount = newExpense.Amount,
                    Date = newExpense.Date,
                    Category = category?.Name ?? string.Empty,
                };

                return expense;
            }

            return await PerformTransactionalOperation(action, token);
        }

        public async Task<ExpenseDTO> DeleteExpense(
            int expenseId,
            int userId,
            CancellationToken token
        )
        {
            async Task<ExpenseDTO> action()
            {
                if (
                    await _repo.GetByIdAsync(expenseId, token) is not Expense expense
                    || expense.UserId != userId
                )
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                expense =
                    await _repo.DeleteAsync(expenseId, token)
                    ?? throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                return new()
                {
                    Id = expense.Id,
                    Category = expense.Category?.Name ?? string.Empty,
                    Description = expense.Description,
                    Amount = expense.Amount,
                    Date = expense.Date,
                };
            }

            return await PerformTransactionalOperation(action, token);
        }

        public async Task<ExpenseDTO> GetExpenseById(
            int expenseId,
            int userId,
            CancellationToken token
        )
        {
            async Task<ExpenseDTO> action()
            {
                if (await _repo.GetByIdAsync(expenseId, token) is not Expense expense)
                    throw new InvalidOperationException(ErrorMessages.NotRegisteredCIncome);

                if (expense.UserId != userId)
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                ExpenseDTO expenseDTO = new()
                {
                    Id = expense.Id,
                    Amount = expense.Amount,
                    Description = expense.Description,
                    Date = expense.Date,
                    Category = expense.Category?.Name ?? string.Empty,
                };

                return expenseDTO;
            }

            return await PerformTransactionalOperation(action, token);
        }

        public async Task<IEnumerable<ExpenseDTO>> GetExpenses(int userId, CancellationToken token)
        {
            async Task<IEnumerable<ExpenseDTO>> action()
            {
                var userIncomes = await _repo.GetExpensesForUser(userId, token);

                List<ExpenseDTO> expenses = [];

                foreach (Expense expense in userIncomes)
                {
                    expenses.Add(
                        new()
                        {
                            Description = expense.Description,
                            Date = expense.Date,
                            Amount = expense.Amount,
                            Category = expense.Category?.Name ?? "",
                            Id = expense.Id,
                        }
                    );
                }

                return expenses;
            }
            return await PerformTransactionalOperation(action, token);
        }

        public async Task<ExpenseDTO> UpdateExpense(
            ExpenseDTO expenseDTO,
            int userId,
            CancellationToken token
        )
        {
            async Task<ExpenseDTO> action()
            {
                if (expenseDTO.Id == null)
                    throw new InvalidOperationException(
                        $"{ErrorMessages.ProvideParater}: {nameof(expenseDTO.Id)}"
                    );

                if (
                    await _repo.GetByIdAsync(expenseDTO.Id.Value, token)
                        is not Expense existingExpense
                    || existingExpense.UserId != userId
                )
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                Category? category = await GetCategory(expenseDTO.Category, userId, token);

                existingExpense.Id = expenseDTO.Id.Value;
                existingExpense.Description = expenseDTO.Description;
                existingExpense.Date = expenseDTO.Date;
                existingExpense.Amount = expenseDTO.Amount;
                existingExpense.CategoryId =
                    category?.Id
                    ?? throw new InvalidOperationException(ErrorMessages.NotRegisteredCategory);

                if (await _repo.UpdateAsync(existingExpense, token) is not Expense updatedExpense)
                    throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                await _unitOfWork.SaveChangesAsync(token);
                await _unitOfWork.CommitTransactionAsync(token);

                return new()
                {
                    Id = updatedExpense.Id,
                    Description = updatedExpense.Description,
                    Amount = updatedExpense.Amount,
                    Date = updatedExpense.Date,
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
