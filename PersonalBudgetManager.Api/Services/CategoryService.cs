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
            int userId,
            CancellationToken token
        )
        {
            async Task<bool> CategoryAlreadyExistAction()
            {
                return await _repo.FindUserCategory(userId, category.Name, token) != null;
            }

            async Task<Category> InsertCategoryAction()
            {
                if (
                    await _repo.InsertAsync(
                        new Category() { Name = category.Name, UserId = userId },
                        token
                    )
                    is not Category newCat
                )
                    throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                return newCat;
            }

            if (await PerformTransactionalOperation(CategoryAlreadyExistAction, token))
                throw new DuplicateNameException(ErrorMessages.RepeatedName);

            Category insertedCategory = await PerformTransactionalOperation(
                InsertCategoryAction,
                token
            );

            return new CategoryDTO() { Name = insertedCategory.Name, Id = insertedCategory.Id };
        }

        public async Task<CategoryDTO> DeleteCategory(
            int categoryId,
            int userId,
            CancellationToken token
        )
        {
            async Task<CategoryDTO> action()
            {
                if (
                    await _repo.GetByIdAsync(categoryId, token) is not Category category
                    || userId != category.UserId
                )
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                category =
                    await _repo.DeleteAsync(categoryId, token)
                    ?? throw new InvalidOperationException(ErrorMessages.UnexpectedError);

                return new CategoryDTO() { Id = category.Id, Name = category.Name };
            }

            return await PerformTransactionalOperation(action, token);
        }

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

        public async Task<CategoryDTO> UpdateCategory(
            CategoryDTO category,
            int userId,
            CancellationToken token
        )
        {
            async Task<CategoryDTO> action()
            {
                if (
                    await _repo.UpdateAsync(
                        new()
                        {
                            Id = category.Id,
                            Name = category.Name,
                            UserId = userId,
                        },
                        token
                    )
                    is not Category updatedCat
                )
                    throw new InvalidOperationException(ErrorMessages.EntryNotFound);
                return new CategoryDTO() { Name = updatedCat.Name, Id = updatedCat.Id };
            }

            return await PerformTransactionalOperation(action, token);
        }
    }
}
