using PersonalBudgetManager.Api.DataContext;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IRepository<T>
        where T : class, IEntity
    {
        public Task<T?> InsertAsync(T entity, CancellationToken token);
        public Task<T?> GetByIdAsync(int id, CancellationToken token);
        public Task<T?> UpdateAsync(T entity, CancellationToken token);
        public Task<T?> DeleteAsync(int id, CancellationToken token);
    }
}
