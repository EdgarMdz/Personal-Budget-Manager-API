namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken token);
        public Task BeginTransactionAsync(CancellationToken token);
        public Task CommitTransactionAsync(CancellationToken token);
        public Task RollbackTransactionAsync(CancellationToken token);
    }
}
