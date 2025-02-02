using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.Repositories.Interfaces
{
    public interface INameableRepository<T> : IRepository<T>
        where T : class, IEntity, IHasNameColumn
    {
        Task<T?> GetByNameAsync(string name, CancellationToken token);
    }
}
