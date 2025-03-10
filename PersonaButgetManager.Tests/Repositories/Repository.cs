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

        private async Task ResetDb(IEnumerable<TestEntity> entities)
        {
            _dbcontext.RemoveRange(_dbcontext.TestEntities);
            await _dbcontext.SaveChangesAsync();

            await _dbcontext.AddRangeAsync(entities);
            await _dbcontext.SaveChangesAsync();
        }

        [Fact]
        public async Task InsertAsync_AddsEntityAndReturnsIt()
        {
            //arrange
            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

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
            DelegateStrategy delegateStrategy = DelegatestrategyFactory.DbUpdateExceptionDelegate(
                exceptionMessage
            );

            var repo = new Repository<TestEntity>(_dbcontext, delegateStrategy);

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
            DelegateStrategy delegateStrategy = DelegatestrategyFactory.ExceptionStrategy(
                exceptionMessage
            );
            var repo = new Repository<TestEntity>(_dbcontext, delegateStrategy);

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

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

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
            await ResetDb([]);

            var token = CancellationToken.None;
            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            // Act
            var deletedEntity = await repo.DeleteAsync(12, token);

            // Assert
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task DeleteAsync_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            DelegateStrategy delegateStrategy = DelegatestrategyFactory.DelayStrategy(5000);
            var repo = new Repository<TestEntity>(_dbcontext, delegateStrategy);

            // Act
            tokenSource.Cancel();
            var task = repo.DeleteAsync(123, token);

            // Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await task);
        }

        [Fact]
        public async Task DeleteAsync_WhenDbUpdateFails_ThrowsException()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var exceptionMessage = "Simulated Exception";
            DelegateStrategy delegateStrategy = DelegatestrategyFactory.DbUpdateExceptionDelegate(
                exceptionMessage
            );
            var repo = new Repository<TestEntity>(_dbcontext, delegateStrategy);

            // Act
            tokenSource.Cancel();
            var task = repo.DeleteAsync(123, token);

            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await task);
            Assert.Contains(exceptionMessage, exception.Message);
            Assert.Empty(_dbcontext.ChangeTracker.Entries());
        }

        [Fact]
        public async Task DeleteASync_GenericExceptionOccurs_ThrowsException()
        {
            // Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var exceptionMessage = "Simulated Exception";
            DelegateStrategy delegateStrategy = DelegatestrategyFactory.ExceptionStrategy(
                exceptionMessage
            );
            var repo = new Repository<TestEntity>(_dbcontext, delegateStrategy);

            // Act
            tokenSource.Cancel();
            var task = repo.DeleteAsync(123, token);

            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await task);
            Assert.Contains(exceptionMessage, exception.Message);
            Assert.Empty(_dbcontext.ChangeTracker.Entries());
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllRegisteredEntities()
        {
            // Arrange
            var newEntities = Enumerable
                .Range(1, 100)
                .Select(i => new TestEntity() { Name = $"Entity {i}" })
                .ToList();

            //cleaning records
            await ResetDb(newEntities);

            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            CancellationToken token = CancellationToken.None;
            int pageNumber = 4;
            int pageSize = 10;

            // Act
            IEnumerable<TestEntity> pagedEntities = await repo.GetAllAsync(
                pageNumber,
                pageSize,
                token
            );

            // Assert
            Assert.Equal(pageSize, pagedEntities.Count());
            Assert.Equal("Entity 31", pagedEntities.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenPageNumberIsOutOfRange_ReturnsEmptyList()
        {
            // Arrange
            var newEntities = Enumerable
                .Range(1, 100)
                .Select(i => new TestEntity() { Name = $"Entity {i}" })
                .ToList();

            await ResetDb(newEntities);

            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());
            var token = CancellationToken.None;
            int pageSize = 10;
            int pageNumber = 100;

            // Act
            var pagedEntities = await repo.GetAllAsync(pageNumber, pageSize, token);

            // Assert
            Assert.Empty(pagedEntities);
        }

        [Fact]
        public async Task GetAllAsync_WhenPageNumberIsNegative_ReturnsEmptyList()
        {
            // Arrange
            var newEtities = Enumerable
                .Range(0, 100)
                .Select(i => new TestEntity() { Name = $"Entity {i}" })
                .ToList();

            await ResetDb(newEtities);

            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            int pageNumber = -1;
            int pageSize = 10;
            var token = CancellationToken.None;

            // Act
            var pagedEntities = await repo.GetAllAsync(pageNumber, pageSize, token);

            // Assert
            Assert.Empty(pagedEntities);
        }

        [Fact]
        public async Task GetAllAsync_WhenNoRecordsInDatabase_ReturnsEmptyList()
        {
            // Arrange
            await ResetDb([]);

            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            int pageNumber = 0;
            int pageSize = 10;
            var token = CancellationToken.None;

            // Act

            var pagedEntities = await repo.GetAllAsync(pageNumber, pageSize, token);

            // Assert
            Assert.Empty(pagedEntities);
        }

        [Fact]
        public async Task GetAllAsync_WhenPageSizeIsZero_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            int pageNumber = 1;
            int pageSize = 0;
            var token = CancellationToken.None;

            // Act and assert
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(
                async () => await repo.GetAllAsync(pageNumber, pageSize, token)
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenPageSizeIsNegative_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            Repository<TestEntity> repo = new(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            int pageNumber = 1;
            int pageSize = -12;
            var token = CancellationToken.None;

            // Act and assert
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(
                async () => await repo.GetAllAsync(pageNumber, pageSize, token)
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenCanceledByTheUser_ThrowsOperationCancelException()
        {
            // Arrange
            Repository<TestEntity> repo = new(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

            int pageNumber = 1;
            int pageSize = 10;
            CancellationTokenSource tokenSource = new();
            var token = tokenSource.Token;

            tokenSource.Cancel();

            // Act and assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await repo.GetAllAsync(pageNumber, pageSize, token)
            );
        }

        [Fact]
        public async Task GetAllAsync_WhenDbUpdateFails_ThrowsException()
        {
            // Arrange
            var exceptionMessage = "Simulated exception";
            Repository<TestEntity> repo = new(
                _dbcontext,
                DelegatestrategyFactory.DbUpdateExceptionDelegate(exceptionMessage)
            );

            var token = CancellationToken.None;
            int pageNumber = 1;
            int pageSize = 1;

            // Act and Assert

            var exception = await Assert.ThrowsAnyAsync<Exception>(
                async () => await repo.GetAllAsync(pageNumber, pageSize, token)
            );

            Assert.Contains(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task GetAllAsync_WhenGenericExceptionOccurs_ThrowsException()
        {
            // Arrange
            string exceptionMessage = "Simulated exception";
            Repository<TestEntity> repo = new(
                _dbcontext,
                DelegatestrategyFactory.ExceptionStrategy(exceptionMessage)
            );

            int pageSize = 1;
            int pageNumber = 1;
            var token = CancellationToken.None;

            // Act and assert
            var exception = await Assert.ThrowsAnyAsync<Exception>(
                async () => await repo.GetAllAsync(pageNumber, pageSize, token)
            );

            Assert.Contains(exceptionMessage, exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdExists_ReturnsEntity()
        {
            // Arrange
            var newEntities = Enumerable
                .Range(1, 10000)
                .Select(i => new TestEntity() { Name = $"Entity {i}", Id = i });

            await ResetDb(newEntities);

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            int id = newEntities.Skip(4093).First().Id;
            var token = CancellationToken.None;

            // Act
            var entity = await repo.GetByIdAsync(id, token);

            // Assert
            Assert.NotNull(entity);
            Assert.Contains(newEntities, e => e.Name == entity.Name);
            Assert.Single(newEntities, e => e.Id == entity.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdNotExists_ReturnsNull()
        {
            // Arrange
            await ResetDb([]);

            int id = 123;
            CancellationToken token = CancellationToken.None;

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            // Act
            var entity = await repo.GetByIdAsync(id, token);

            // Assert
            Assert.Null(entity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenIdIsNegative_ThrowsArgumentException()
        {
            // Arrange
            await ResetDb([]);

            int id = -112;
            CancellationToken token = CancellationToken.None;

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            // Act and assert

            var ex = await Assert.ThrowsAnyAsync<ArgumentException>(
                async () => await repo.GetByIdAsync(id, token)
            );
            Assert.Contains("Id must be greater than 0", ex.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            //Arrange
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var id = 123;

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

            // Act and assert
            tokenSource.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await repo.GetByIdAsync(id, token)
            );
        }

        [Fact]
        public async Task GetByIdAsync_WhenDbUpdateFails_ThrowsAsync()
        {
            //Arrange
            var id = 123;
            var token = CancellationToken.None;
            string exceptionMessage = "Simulated Exception";
            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.DbUpdateExceptionDelegate(exceptionMessage)
            );

            // Act and assert
            var ex = await Assert.ThrowsAnyAsync<Exception>(
                async () => await repo.GetByIdAsync(id, token)
            );

            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenGenericExceptionOccurs_ThrowsException()
        {
            //Arrage
            string exceptionMessage = "Simulated Exception";
            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.ExceptionStrategy(exceptionMessage)
            );

            int id = 123;
            var token = CancellationToken.None;

            // Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.GetByIdAsync(id, token)
            );

            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_WhenIdExist_ReturnsIt()
        {
            //Arrange
            var newEntities = Enumerable
                .Range(1, 1000)
                .Select(i => new TestEntity() { Name = $"Entity {i}", Id = i });

            await ResetDb(newEntities);

            var repo = new Repository<TestEntity>(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            var id = 50;
            string newName = "Updated entity";
            var token = CancellationToken.None;
            var entity =
                await repo.GetByIdAsync(id, token)
                ?? throw new InvalidOperationException("Entity with ID {id} not found.");

            entity.Name = newName;

            //Act
            var updatedEntity = await repo.UpdateAsync(entity, token);

            //Assert
            Assert.NotNull(updatedEntity);
            Assert.Equal(id, updatedEntity.Id);
            Assert.Equal(newName, updatedEntity.Name);

            var entityFromDb = await repo.GetByIdAsync(id, token);
            Assert.NotNull(entityFromDb);
            Assert.Equal(id, entityFromDb.Id);
            Assert.Equal(newName, entityFromDb.Name);
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
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options
            ) { }

        public DbSet<TestEntity> TestEntities { get; set; }
    }

    public static class DelegatestrategyFactory
    {
        public static DelegateStrategy NoOpStrategy() => new((ct) => Task.CompletedTask);

        public static DelegateStrategy DelayStrategy(int miliSeconds) =>
            new((ct) => Task.Delay(miliSeconds, ct));

        public static DelegateStrategy ExceptionStrategy(string message) =>
            new((ct) => throw new Exception(message));

        public static DelegateStrategy DbUpdateExceptionDelegate(string message) =>
            new((ct) => throw new DbUpdateException(message));
    }
}
