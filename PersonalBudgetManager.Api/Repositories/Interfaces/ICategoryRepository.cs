using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        public Task<Category?> FindUserCategory(
            int userId,
            string category,
            CancellationToken token
        );
    }
}
