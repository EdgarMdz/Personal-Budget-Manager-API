using PersonaButgetManager.Tests.Common.Factories;
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

        [Fact]
        public async Task FindUserCategory_WhenUserIdAmongMultipleUsersAndCategoryNameExist_ReturnsLastCategory()
        {
            // Arrange
            await ResetDb(0);

            var _categoryRepository = new PersonalBudgetManager.Api.Repositories.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );
            var token = CancellationToken.None;
            var categories = Enumerable
                .Range(1, 100)
                .Select(i => new Category()
                {
                    Id = i,
                    Name = $"TestCategory {i}",
                    UserId = i,
                });
            await _dbcontext.Categories.AddRangeAsync(categories);
            await _dbcontext.SaveChangesAsync();

            var category = _dbcontext.Categories.Last();

            //Act
            var result = await _categoryRepository.FindUserCategory(
                category.UserId,
                category.Name,
                token
            );

            //Assert
            Assert.NotNull(result);
            Assert.Equal(category.UserId, result.UserId);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Id, result.Id);
        }
    }
}
