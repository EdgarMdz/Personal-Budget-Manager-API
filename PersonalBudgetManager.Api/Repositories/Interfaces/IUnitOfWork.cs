using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public IIncomeRepository IncomeRepository { get; }

        public Repository<T> GetRepository<T>()
            where T : class, IEntity;
        public NameableRepository<T> GetNameableRepository<T>()
            where T : class, IEntity, IHasNameColumn;

        public Task<int> SaveChangesAsync(CancellationToken token);
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token);
        public Task CommitTransactionAsync(CancellationToken token);
        public Task RollbackTransactionAsync(CancellationToken token);
    }
}
