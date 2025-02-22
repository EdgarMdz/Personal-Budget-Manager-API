using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Interfaces;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class UnitOfWork(
        AppDbContext appDbContext,
        IIncomeRepository incomeRepository,
        ICategoryRepository categoryRepository,
        IExpensesRepository expensesRepository
    ) : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _appDbContext = appDbContext;
        private readonly Dictionary<Type, object> _repositories = [];

        public IIncomeRepository IncomeRepository { get; } = incomeRepository;

        public ICategoryRepository CategoryRepository { get; } = categoryRepository;

        public IExpensesRepository ExpensesRepository { get; } = expensesRepository;

        public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken token) =>
            await _appDbContext.Database.BeginTransactionAsync(token);

        public async Task CommitTransactionAsync(CancellationToken token) =>
            await _appDbContext.Database.CommitTransactionAsync(token);

        public async Task RollbackTransactionAsync(CancellationToken token) =>
            await _appDbContext.Database.RollbackTransactionAsync(token);

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

        public NameableRepository<T> GetNameableRepository<T>()
            where T : class, IEntity, IHasNameColumn
        {
            var repoType = typeof(T);

            if (_repositories.TryGetValue(repoType, out object? value))
                return (NameableRepository<T>)value;
            else
            {
                NameableRepository<T> repo = new(_appDbContext);
                _repositories.Add(repoType, repo);
                return repo;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken token) =>
            await _appDbContext.SaveChangesAsync(token);
    }
}
