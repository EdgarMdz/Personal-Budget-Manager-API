namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken token);
    }
}
