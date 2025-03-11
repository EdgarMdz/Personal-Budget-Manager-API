using PersonaButgetManager.Tests.Common.Entities;
using PersonaButgetManager.Tests.Common.Factories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class NameableRepository : BaseTest
    {
        [Fact]
        public async Task GetByNameAsync_WhenNameExists_ReturnsEntity()
        {
            // Arrange
            var newEntities = await ResetDb(10000);
            var repo = new PersonalBudgetManager.Api.Repositories.NameableRepository<TestEntity>(
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
    }
}
