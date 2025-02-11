using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetUserCategories(int id, CancellationToken token);
        Task<Category?> GetUserCategory(int userId, string categoryName, CancellationToken token);
    }
}
