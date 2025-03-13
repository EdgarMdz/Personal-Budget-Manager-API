using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonaButgetManager.Tests.Repositories
{
    public class CategoryRepository : BaseTest
    {
        public CategoryRepository()
            : base() { }

        [Fact]
        public async Task FindUserCategory_WhenUserIdAndCategoryNameExist_ReturnsCategory()
        {
            // Arrange
            var _categoryRepository = new PersonalBudgetManager.Api.Repositories.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );
            var token = CancellationToken.None;
            var userId = 1;
            var categoryName = "TestCategory";
            var category = new Category
            {
                Id = 1,
                Name = categoryName,
                UserId = userId,
            };
            await _dbcontext.Categories.AddAsync(category);
            await _dbcontext.SaveChangesAsync();

            // Act
            var result = await _categoryRepository.FindUserCategory(userId, categoryName, token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categoryName, result.Name);
            Assert.Equal(userId, result.UserId);
        }
    }
}
