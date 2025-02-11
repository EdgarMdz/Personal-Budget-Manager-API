using System.Data;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class CategoryService(IUnitOfWork unitOfWork) : BaseService(unitOfWork), ICategoryService
    {
        private readonly ICategoryRepository _repo = unitOfWork.CategoryRepository;

        public async Task<CategoryDTO> AddCategory(
            CategoryDTO category,
            int id,
            CancellationToken token
        ) =>
            await PerformTransactionalOperation(
                async () =>
                {
                    if (await _repo.FindUserCategory(id, category.Name, token) is not null)
                        throw new DuplicateNameException(ErrorMessages.RepeatedName);

                    if (
                        await _repo.InsertAsync(
                            new Category() { Name = category.Name, UserId = id },
                            token
                        )
                        is not Category newCat
                    )
                        throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                    return new CategoryDTO() { Name = newCat.Name, Id = newCat.Id };
                },
                token
            );

        public async Task<IEnumerable<CategoryDTO>> GetUserCategories(
            int userId,
            CancellationToken token
        ) =>
            await PerformTransactionalOperation(
                async () =>
                {
                    var categories = await _repo.GetCategoriesForUser(userId, token);

                    List<CategoryDTO> categoryDTOs = [];
                    foreach (Category category in categories)
                    {
                        categoryDTOs.Add(new() { Id = category.Id, Name = category.Name });
                    }

                    return categoryDTOs;
                },
                token
            );

        public async Task<Category?> GetUserCategory(
            int userId,
            string categoryName,
            CancellationToken token
        ) =>
            await PerformTransactionalOperation(
                async () => await _repo.FindUserCategory(userId, categoryName, token),
                token
            );
    }
}
