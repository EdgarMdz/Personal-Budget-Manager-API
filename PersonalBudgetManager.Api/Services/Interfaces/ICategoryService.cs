using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO> AddCategory(CategoryDTO category, int userId, CancellationToken token);
        Task<CategoryDTO> DeleteCategory(int categoryId, int userId, CancellationToken token);
        Task<IEnumerable<CategoryDTO>> GetUserCategories(int userId, CancellationToken token);
        Task<Category?> GetUserCategory(int userId, string categoryName, CancellationToken token);
        Task<CategoryDTO> UpdateCategory(CategoryDTO category, int userId, CancellationToken token);
    }
}
