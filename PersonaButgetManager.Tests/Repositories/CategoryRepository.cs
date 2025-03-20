using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.DataContext.Entities;
using CategpryRepoAPI = PersonalBudgetManager.Api.Repositories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class CategoryRepository : BaseTest
    {
        public CategoryRepository()
            : base() { }

        [Fact]
        public async Task FindUserCategory_WhenUserIdAmongMultipleUsersAndCategoryNameExist_ReturnsLastCategory()
        {
            // Arrange
            await ResetDb(0);

            var _categoryRepository = new CategpryRepoAPI.CategoryRepository(
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

        [Fact]
        public async Task FindUserCategory_WhenUserIdAmongMultipleUsersAndCategoryNameExist_ReturnsMiddleCategory()
        {
            //Arrange
            await ResetDb(0);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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

            var category = _dbcontext.Categories.Skip(categories.Count() / 2).Take(1).First();
            var token = CancellationToken.None;

            //Act
            var result = await repo.FindUserCategory(category.UserId, category.Name, token);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(category.UserId, result.UserId);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Id, result.Id);
        }

        [Fact]
        public async Task FindUserCategory_WhenUserIdNotExistsAndCategoryNameExist_ReturnsNull()
        {
            //Arrange
            await ResetDb(0);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );
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

            var category = new Category() { UserId = 500, Name = "TestCategory 50" };
            var token = CancellationToken.None;

            //Act

            var result = await repo.FindUserCategory(category.UserId, category.Name, token);

            //Assert
            Assert.Null(result);
        }
    }
}
