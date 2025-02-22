using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class RepositoryTests : IDisposable
    {
        private readonly TestDBContext _dbcontext;

        public RepositoryTests()
        {
            _dbcontext = new TestDBContext();
        }

        public void Dispose()
        {
            _dbcontext.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public async Task InsertAsync_AddsEntityAndReturnsIt()
        {
            //arrange
            var repo = new Repository<TestEntity>(_dbcontext);
            string entityName = "Test entity";
            var newEntity = new TestEntity() { Name = entityName };
            var token = CancellationToken.None;

            //act
            var insertedEntity = await repo.InsertAsync(newEntity, token);
            await _dbcontext.SaveChangesAsync(token);

            //assert
            Assert.NotNull(insertedEntity);
            Assert.Equal(newEntity.Id, insertedEntity.Id);
            Assert.Equal(newEntity.Name, insertedEntity.Name);

            var entityFromDb = await _dbcontext.Set<TestEntity>().FindAsync(insertedEntity.Id);
            Assert.NotNull(entityFromDb);
            Assert.Equal(entityName, entityFromDb.Name);
        }

        [Fact]
        public async Task InsertAsync_WhenCanceledByUser_ThrowsOperationCanceledException()
        {
            // Arrange
            var testEntity = new TestEntity { Name = "Test entity" };
            using var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var delayProvider = new RealDelayProvider();
            var repo = new Repository<TestEntity>(_dbcontext, delayProvider);

            // Act
            cancellationTokenSource.Cancel();
            var task = repo.InsertAsync(testEntity, token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        [Fact]
        public async Task InsertAsync_WhenDbUpdateFails_ThrowsException()
        {
            // Arrange
            var testEntity = new TestEntity() { Name = "Test entity" };
            var token = CancellationToken.None;
            string exceptionMessage = "Simulated DB error";
            DbUpdateExceptionThrower exceptionThrower = new(exceptionMessage);
            var repo = new Repository<TestEntity>(_dbcontext, exceptionThrower: exceptionThrower);

            // Act and assert
            var ex = await Assert.ThrowsAnyAsync<Exception>(
                async () => await repo.InsertAsync(testEntity, token)
            );

            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task InsertAsync_WhenGenericExceptionOccurs_ThrowsException_AndContextIsClean()
        {
            //arrange
            var testEntity = new TestEntity() { Name = "Test entity" };
            var token = CancellationToken.None;

            var exceptionMessage = "Generic exception thrown for testing purposes";
            GenericExceptionThrower exceptionThrower = new(exceptionMessage);
            var repo = new Repository<TestEntity>(_dbcontext, exceptionThrower: exceptionThrower);

            //Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.InsertAsync(testEntity, token)
            );

            Assert.Contains($"An error occurred", ex.Message);
            Assert.Empty(_dbcontext.ChangeTracker.Entries());
        }

        [Fact]
        public async Task DeleteAsync_DeleteEntityAndReturnsIt()
        {
            // Arrange
            TestEntity testEntity = new() { Name = "Test entity" };
            _dbcontext.TestEntities.Add(testEntity);
            await _dbcontext.SaveChangesAsync();

            var repo = new Repository<TestEntity>(_dbcontext);

            // Act
            var deletedEntity = await repo.DeleteAsync(testEntity.Id, CancellationToken.None);

            await _dbcontext.SaveChangesAsync();

            // Assert
            Assert.NotNull(deletedEntity);
            Assert.Equal(testEntity.Id, deletedEntity.Id);
            Assert.Null(await _dbcontext.TestEntities.FindAsync(testEntity.Id));
            Assert.Empty(_dbcontext.ChangeTracker.Entries());
        }

        [Fact]
        public async Task DeleteAsync_WhenIdDoesNotExists_ReturnNull()
        {
            //Arrange
            var token = CancellationToken.None;
            var repo = new Repository<TestEntity>(_dbcontext);

            // Act
            var deletedEntity = await repo.DeleteAsync(12, token);

            // Assert
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteASync_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var delayProvider = new RealDelayProvider();
            var repo = new Repository<TestEntity>(_dbcontext, delayProvider);

            // Act
            tokenSource.Cancel();
            var task = repo.DeleteAsync(123, token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRegisteredEntities()
        {
            // Arrange
            var newEntities = new TestEntity[]
            {
                new() { Name = "First Entity" },
                new() { Name = "Second Entity" },
                new() { Name = "Third Entity" },
            };

            //cleaning records
            _dbcontext.TestEntities.RemoveRange(_dbcontext.TestEntities);
            await _dbcontext.SaveChangesAsync();

            await _dbcontext.TestEntities.AddRangeAsync(newEntities);

            await _dbcontext.SaveChangesAsync();

            Repository<TestEntity> repo = new(_dbcontext);

            CancellationToken token = CancellationToken.None;

            // Act

            IEnumerable<TestEntity> entities = await repo.GetAllAsync(token);

            var expectedEntities = newEntities.OrderBy(e => e.Name).ToList();
            var actualEntities = entities.OrderBy(e => e.Name).ToList();

            // Assert

            Assert.Equal(expectedEntities.Count, actualEntities.Count);

            for (int i = 0; i < expectedEntities.Count; i++)
            {
                Assert.Equal(expectedEntities[i].Id, actualEntities[i].Id);
                Assert.Equal(expectedEntities[i].Name, actualEntities[i].Name);
            }
        }
    }

    public class TestEntity : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }

    public class TestDBContext : AppDbContext
    {
        public TestDBContext()
            : base(
                new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase("TestDB").Options
            ) { }

        public DbSet<TestEntity> TestEntities { get; set; }
    }
}
