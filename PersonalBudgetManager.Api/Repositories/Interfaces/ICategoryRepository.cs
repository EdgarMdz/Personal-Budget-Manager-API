using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        public Task<Category?> FindUserCategory(
            int userId,
            string category,
            CancellationToken token
        );
        Task<IEnumerable<Category>> GetCategoriesForUser(int userId, CancellationToken token);
    }
}
