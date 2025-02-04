using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<Category?> GetUserCategory(int userId, string categoryName, CancellationToken token);
    }
}
