using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class Repository<T>(AppDbContext context) : IRepository<T>
        where T : class, IEntity
    {
        private readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<T?> DeleteAsync(int id, CancellationToken token) =>
            await PerformDatabaseOperation(async () =>
            {
                T? entity = await GetByIdAsync(id, token);
                if (entity != null)
                    _dbSet.Remove(entity);
                return entity;
            });

        public async Task<T?> GetByIdAsync(int id, CancellationToken token)
        {
            if (id < 0)
                throw new ArgumentException("Id must be greater than 0", nameof(id));

            return await PerformDatabaseOperation(async () => await _dbSet.FindAsync([id], token));
        }

        public async Task<T?> InsertAsync(T entity, CancellationToken token) =>
            await PerformDatabaseOperation(async () =>
            {
                var result = await _dbSet.AddAsync(entity, token);
                return result.Entity;
            });

        public Task<T?> UpdateAsync(T entity, CancellationToken token)
        {
            return PerformDatabaseOperation(async () =>
            {
                var existingEntity = await GetByIdAsync(entity.Id, token);
                if (existingEntity is not null)
                {
                    _dbSet.Update(entity);
                    return entity;
                }
                return null;
            });
        }

        protected static async Task<TResult> PerformDatabaseOperation<TResult>(
            Func<Task<TResult>> action
        )
        {
            try
            {
                return await action();
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exceptions
                // Log the exception or rethrow it
                throw new Exception("An error occurred while accessing the database.", ex);
            }
            catch (OperationCanceledException ex)
            {
                // Handle operation canceled exceptions
                // Log the exception or rethrow it
                throw new Exception("The operation was canceled.", ex);
            }
            catch (Exception ex)
            {
                // Handle all other exceptions
                // Log the exception or rethrow it
                throw new Exception("An unexpected error occurred.", ex);
            }
        }

        public Task<IEnumerable<T>> GetAllAsync(CancellationToken token) =>
            PerformDatabaseOperation<IEnumerable<T>>(async () => await _dbSet.ToListAsync(token));
    }
}
