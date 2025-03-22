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
            var categories = await ResetDb<Category>(100);

            var _categoryRepository = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );
            var token = CancellationToken.None;
            var category = categories.Last();

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
            var categories = await ResetDb<Category>(100);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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
            var categories = await ResetDb<Category>(100);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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
            await ResetDb<Category>(100);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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
            await ResetDb<Category>(0);

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
            await ResetDb<Category>(0);

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

        [Fact]
        public async Task FindUserCategory_WhenDBUpdateFails_ThrowsException()
        {
            // Arrange
            await ResetDb<Category>(0);

            var exceptionMessage = "simulated exception";

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.DbUpdateExceptionDelegate(exceptionMessage)
            );

            var token = CancellationToken.None;
            var categoryName = "TestCategory";
            var userid = 50;

            //Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.FindUserCategory(userid, categoryName, token)
            );
            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task GetCategoriesForUser_WhenUserExistAndHasCategories_ReturnsCategories()
        {
            // Arrange
            var token = CancellationToken.None;
            var userid = 50;

            var categories = await ResetDb<Category>(100);
            var extraCategories = Enumerable
                .Range(1, 5)
                .Select(i => new Category()
                {
                    UserId = userid,
                    Name = $"TestCategory {categories.Count() + i}",
                    Id = categories.Count() + i,
                });

            await _dbcontext.Categories.AddRangeAsync(extraCategories);
            await _dbcontext.SaveChangesAsync();

            var expectedCategories = categories
                .Where(C => C.UserId == userid)
                .Concat(extraCategories);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            //Act
            var result = await repo.GetCategoriesForUser(userid, token);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCategories.Count(), result.Count());

            Assert.All(
                result,
                category =>
                {
                    var matchingCategory = expectedCategories.FirstOrDefault(c =>
                        c.Id == category.Id
                    );

                    Assert.NotNull(matchingCategory);
                    Assert.Equal(userid, category.UserId);
                    Assert.Contains(category.Name, matchingCategory.Name);
                }
            );
        }

        [Fact]
        public async Task GetCategoriesForUser_WhenUserIdNotExist_ReturnsNull()
        {
            // Arrange
            await ResetDb<Category>(100);
            var userid = 500;
            var token = CancellationToken.None;

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            //Act
            var result = await repo.GetCategoriesForUser(userid, token);

            //Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCategoriesForUser_WhenCanceledByUser_ThrowsOperationCanceledException()
        {
            // Arrange
            await ResetDb<Category>(0);

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

            var cancellationTokenSource = new CancellationTokenSource();
            var toke = cancellationTokenSource.Token;
            var userId = 50;

            // Act and assert
            cancellationTokenSource.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await repo.GetCategoriesForUser(userId, toke)
            );
        }

        [Fact]
        public async Task GetCategoriesForUser_WhenGenericExceptionOccurs_ThrowException()
        {
            // Arrage
            await ResetDb<Category>(0);

            var exceptionMessage = "Simulated exception";
            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.ExceptionStrategy(exceptionMessage)
            );
            var token = CancellationToken.None;
            var userId = 50;

            //Act and assert

            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.GetCategoriesForUser(userId, token)
            );
            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task GetCategoriesForUser_WhenDBUpdateFails_ThrowsException()
        {
            // Arrange
            await ResetDb<Category>(0);

            var exceptionMessage = "Simulated exception";

            var repo = new CategpryRepoAPI.CategoryRepository(
                _dbcontext,
                DelegatestrategyFactory.DbUpdateExceptionDelegate(exceptionMessage)
            );

            var token = CancellationToken.None;
            var userid = 50;

            // Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.GetCategoriesForUser(userid, token)
            );

            Assert.Contains(exceptionMessage, ex.Message);
        }
    }
}
