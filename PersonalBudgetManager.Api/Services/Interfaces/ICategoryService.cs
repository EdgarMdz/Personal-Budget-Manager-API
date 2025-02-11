using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> AddCategory(CategoryDTO category, int id, CancellationToken token);
        Task<IEnumerable<CategoryDTO>> GetUserCategories(int id, CancellationToken token);
        Task<Category?> GetUserCategory(int userId, string categoryName, CancellationToken token);
    }
}
