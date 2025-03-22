using System.Threading.Tasks;
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

        [Fact]
        public async Task FindUserCategory_WhenUserIdExistsAndCategoryNameNot_ReturnsNull()
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
                    Name = $"TestCategory {i}",
                    UserId = i,
                    Id = i,
                });
            await _dbcontext.Categories.AddRangeAsync(categories);
            await _dbcontext.SaveChangesAsync();

            var category = new Category() { UserId = 50, Name = "TestCategory 500" };
            var token = CancellationToken.None;

            //Act
            var result = await repo.FindUserCategory(category.UserId, category.Name, token);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FindUserCategory_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            // Arrange
            await ResetDb(0);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

            var category = new Category()
            {
                UserId = 50,
                Name = "TestCategory 50",
                Id = 50,
            };
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            //Act and assert
            cancellationTokenSource.Cancel();

            var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await repo.FindUserCategory(category.UserId, category.Name, token)
            );

            Assert.NotNull(ex);
        }

        [Fact]
        public async Task FindUserCategory_WhenGenericExceptionOccurs_ThrowsException()
        {
            // Arrange
            await ResetDb(0);

            var message = "Simuiltaed exception";
            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.ExceptionStrategy(message)
            );

            var token = CancellationToken.None;
            var categoryName = "TestCategory 50";
            var userId = 50;

            //Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.FindUserCategory(userId, categoryName, token)
            );

            Assert.NotNull(ex);
            Assert.Contains(message, ex.Message);
        }
    }
}
