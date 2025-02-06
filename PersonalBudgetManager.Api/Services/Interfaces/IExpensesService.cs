using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IExpensesService
    {
        Task<ExpenseDTO> AddExpense(
            ExpenseDTO expenseDTO,
            int categoryId,
            int userId,
            CancellationToken token
        );
        Task<ExpenseDTO> DeleteExpense(int expenseId, int userId, CancellationToken token);
        public Task<ExpenseDTO> GetExpenseById(int expenseId, int userId, CancellationToken token);
        public Task<IEnumerable<ExpenseDTO>> GetExpenses(int userId, CancellationToken token);
        public Task<ExpenseDTO> UpdateExpense(
            ExpenseDTO expenseDTO,
            int userId,
            CancellationToken token
        );
    }
}
