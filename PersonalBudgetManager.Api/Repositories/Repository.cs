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
        private readonly IDelayProvider _delayProvider;

        public Repository(AppDbContext context, IDelayProvider delayProvider)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _delayProvider = delayProvider;
        }

        public async Task<T?> DeleteAsync(int id, CancellationToken token) =>
            await PerformDatabaseOperation(async () =>
            {
                T? entity = await GetByIdAsync(id, token);
                if (entity != null)
                    _dbSet.Remove(entity);
                return entity;
            });

        public Task<IEnumerable<T>> GetAllAsync(CancellationToken token) =>
            PerformDatabaseOperation<IEnumerable<T>>(async () => await _dbSet.ToListAsync(token));

        public async Task<T?> GetByIdAsync(int id, CancellationToken token)
        {
            if (id < 0)
                throw new ArgumentException("Id must be greater than 0", nameof(id));

            return await PerformDatabaseOperation(async () => await _dbSet.FindAsync([id], token));
        }

        public async Task<T> InsertAsync(T entity, CancellationToken token) =>
            await PerformDatabaseOperation(async () =>
            {
                await _delayProvider.DelayAsync(TimeSpan.FromMilliseconds(100), token);
                var result = await _dbSet.AddAsync(entity, token);
                return result.Entity;
            });

        public Task<T?> UpdateAsync(T entity, CancellationToken token)
        {
            return PerformDatabaseOperation(async () =>
            {
                var entityExist = await _dbSet.AnyAsync(c => c.Id == entity.Id, token);
                if (entityExist)
                {
                    _dbSet.Update(entity);
                    return entity;
                }
                return null;
            });
        }

        protected async Task<TResult> PerformDatabaseOperation<TResult>(
            Func<Task<TResult>> action
        ) => await PerformDatabaseOperationHelper(action, _context);

        protected static async Task<TResult> PerformDatabaseOperationHelper<TResult>(
            Func<Task<TResult>> action,
            AppDbContext context
        )
        {
            try
            {
                return await action();
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
