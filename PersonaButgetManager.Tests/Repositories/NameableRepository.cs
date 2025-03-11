using PersonaButgetManager.Tests.Common.Entities;
using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.Repositories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class NameableRepository : BaseTest
    {
        [Fact]
        public async Task GetByNameAsync_WhenNameExists_ReturnsEntity()
        {
            // Arrange
            var newEntities = await ResetDb(10000);
            var repo = new NameableRepository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            var entity = newEntities.Last();
            var token = CancellationToken.None;

            //Act
            var result = await repo.GetByNameAsync(entity.Name, token);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
        }

        [Fact]
        public async Task GetByNameAsync_WhenNameNotExists_ReturnsNull()
        {
            //Arrange
            await ResetDb(10000);
            var repo = new NameableRepository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            var token = CancellationToken.None;
            string name = "NotExists";

            //Act
            var result = await repo.GetByNameAsync(name, token);

            //Assert
            Assert.Null(result);
        }
    }
}
