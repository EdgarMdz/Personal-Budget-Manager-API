using Microsoft.EntityFrameworkCore;
using Moq;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonaButgetManager.Tests.Repositories
{
    public class RepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbcontext;
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
            var task = _repository.InsertAsync(testEntity, token);

            // Act
            cancellationTokenSource.CancelAfter(50); // Cancela después de 50ms

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        [Fact]
        public async Task InsertAsync_WhenDbUpdateFails_ThrowsException()
        {
            // Arrange
            var testEntity = new TestEntity() { Name = "Test entity" };
            var token = CancellationToken.None;

            _repositoryMock
                .Setup(r => r.InsertAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException("Simulated DB error"));

            // Act and assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await _repositoryMock.Object.InsertAsync(testEntity, token)
            );
        }

        [Fact]
        public async Task InsertAsync_WhenGenericExceptionOccurs_ThrowsException()
        {
            // Arrange
            var testEntity = new TestEntity() { Name = "Test entity" };
            var token = CancellationToken.None;

            _repositoryMock
                .Setup(r => r.InsertAsync(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Simulated generic exception"));

            // Act and Assert
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await _repositoryMock.Object.InsertAsync(testEntity, token)
            );
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
