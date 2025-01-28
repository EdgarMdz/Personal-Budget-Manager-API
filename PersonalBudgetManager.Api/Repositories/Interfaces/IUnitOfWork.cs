using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public Repository<T> GetRepository<T>()
            where T : class, IEntity;
        public Task<int> SaveChangesAsync(CancellationToken token);
        public Task BeginTransactionAsync(CancellationToken token);
        public Task CommitTransactionAsync(CancellationToken token);
        public Task RollbackTransactionAsync(CancellationToken token);
    }
}
