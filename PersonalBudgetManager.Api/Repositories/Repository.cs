using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class Repository<T> : IRepository<T>
        where T : class, IEntity
    {
        private readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        private readonly IDelayProvider? _delayProvider;
        private readonly IExceptionThrower? _exceptionThrower;

        public Repository(
            AppDbContext context,
            IDelayProvider? delayProvider = null,
            IExceptionThrower? exceptionThrower = null
        )
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _delayProvider = delayProvider;
            _exceptionThrower = exceptionThrower;
        }

        public async Task<T?> DeleteAsync(int id, CancellationToken token)
        {
            async Task<T?> action(CancellationToken ct)
            {
                T? entity = await GetByIdAsync(id, ct);
                if (entity != null)
                    _dbSet.Remove(entity);
                return entity;
            }

            return await PerformDatabaseOperation(action, token);
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            int pageNumber,
            int pageSize,
            CancellationToken token
        )
        {
            async Task<IEnumerable<T>> action(CancellationToken ct) =>
                await _dbSet.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return await PerformDatabaseOperation(action, token);
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken token)
        {
            if (id < 0)
                throw new ArgumentException("Id must be greater than 0", nameof(id));

            async Task<T?> action(CancellationToken ct) => await _dbSet.FindAsync([id], ct);

            return await PerformDatabaseOperation(action, token);
        }

        public async Task<T> InsertAsync(T entity, CancellationToken token)
        {
            async Task<T> action(CancellationToken ct)
            {
                var result = await _dbSet.AddAsync(entity, ct);
                return result.Entity;
            }
            return await PerformDatabaseOperation(action, token);
        }

        public async Task<T?> UpdateAsync(T entity, CancellationToken token)
        {
            async Task<T?> action(CancellationToken ct)
            {
                var entityExist = await _dbSet.AnyAsync(c => c.Id == entity.Id, ct);
                if (entityExist)
                {
                    _dbSet.Update(entity);
                    return entity;
                }
                return null;
            }
            return await PerformDatabaseOperation(action, token);
        }

        protected async Task<TResult> PerformDatabaseOperation<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken token
        )
        {
            async Task<TResult> adaptedAction(CancellationToken ctoken)
            {
                if (_delayProvider != null)
                    await _delayProvider.DelayAsync(TimeSpan.FromMilliseconds(1000), ctoken);

                _exceptionThrower?.ThrowException();

                return await action(ctoken);
            }

            return await PerformDatabaseOperationHelper(adaptedAction, token, _context);
        }

        protected static async Task<TResult> PerformDatabaseOperationHelper<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken token,
            AppDbContext context
        )
        {
            try
            {
                return await action(token);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                context.ChangeTracker.Clear();
                throw new Exception(
                    $"An error occurred while accessing the database: {ex.Message}",
                    ex
                );
            }
            catch (OperationCanceledException ex)
            {
                // Handle operation canceled exceptions
                // Log the exception or rethrow it
                throw new OperationCanceledException("The operation was canceled.", ex);
            }
            catch (Exception ex)
            {
                // Handle all other exceptions
                context.ChangeTracker.Clear();
                throw new Exception($"An error occurred: {ex.Message}", ex);
            }
        }
    }
}
