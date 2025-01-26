using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class UnitOfWork(AppDbContext appDbContext) : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _appDbContext = appDbContext;
        private readonly Dictionary<Type, object> _repositories = [];

        public void Dispose()
        {
            _appDbContext.Dispose();
            GC.SuppressFinalize(this);
        }

        public Repository<T> GetRepository<T>()
            where T : class, IEntity
        {
            var repoType = typeof(T);

            if (_repositories.TryGetValue(repoType, out object? value))
                return (Repository<T>)value;
            else
            {
                Repository<T> repo = new(_appDbContext);
                _repositories.Add(repoType, repo);
                return repo;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken token) =>
            await _appDbContext.SaveChangesAsync(token);
    }
}
