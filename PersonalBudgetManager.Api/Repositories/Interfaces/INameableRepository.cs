using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface INameableRepository<T>
        where T : class, IHasNameColumn
    {
        Task<T?> GetByNameAsync(string name, CancellationToken token);
    }
}
