using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class CategoryService(IUnitOfWork unitOfWork) : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICategoryRepository _repo = unitOfWork.CategoryRepository;

        public async Task<Category?> GetUserCategory(
            int userId,
            string categoryName,
            CancellationToken token
        ) => await _repo.FindUserCategory(userId, categoryName, token);
    }
}
