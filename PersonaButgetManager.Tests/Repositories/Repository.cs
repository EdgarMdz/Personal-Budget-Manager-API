using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using Xunit.Sdk;

namespace PersonaButgetManager.Tests.Repositories
{
    public class RepositoryTests : IDisposable
    {
        private readonly TestDBContext _dbcontext;
        private readonly Repository<TestEntity> _repository;
        private readonly Mock<IRepository<TestEntity>> _repositoryMock;

        public RepositoryTests()
        {
            _dbcontext = new TestDBContext();
            _repository = new(_dbcontext, new RealDelayProvider());
            _repositoryMock = new();
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
            string entityName = "Test entity";
            var newEntity = new TestEntity() { Name = entityName };
            var token = CancellationToken.None;

            //act
            var insertedEntity = await _repository.InsertAsync(newEntity, token);
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

            // Act
            cancellationTokenSource.Cancel();
            var task = _repository.InsertAsync(testEntity, token);

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

            _repositoryMock
                .Setup(r => r.InsertAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException(exceptionMessage));

            // Act and assert
            var ex = await Assert.ThrowsAnyAsync<Exception>(
                async () => await _repositoryMock.Object.InsertAsync(testEntity, token)
            );
        }

        [Fact]
        public async Task InsertAsync_WhenGenericExceptionOccurs_ThrowsException_AndContextIsClean()
        {
            //arrange
            var testEntity = new TestEntity() { Name = "Test entity" };
            var token = CancellationToken.None;

            var dbContext = new TestDBContext();
            var repo = new Repository<TestEntity>(dbContext, null);

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

            // Act
            var deletedEntity = await _repository.DeleteAsync(
                testEntity.Id,
                CancellationToken.None
            );

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

            // Act
            var deletedEntity = await _repository.DeleteAsync(12, token);

            // Assert
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteASync_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            // Act
            tokenSource.Cancel();
            var task = _repository.DeleteAsync(123, token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
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
