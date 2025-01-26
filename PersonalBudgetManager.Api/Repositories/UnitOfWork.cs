using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Repositories
{
    public class UnitOfWork(
        AppDbContext appDbContext,
        Repository<User> userRepo,
        Repository<Category> categoryRepo,
        Repository<Income> incomeRepo,
        Repository<Expense> expenseRepo
    ) : IUnitOfWork
    {
        private readonly AppDbContext _appDbContext = appDbContext;
        public Repository<User> UserRepository { get; } = userRepo;
        public Repository<Category> CategoryRepository { get; } = categoryRepo;
        public Repository<Income> IncomeRepository { get; } = incomeRepo;
        public Repository<Expense> ExpenceRepository { get; } = expenseRepo;

        public async Task<int> SaveChangesAsync(CancellationToken token) =>
            await _appDbContext.SaveChangesAsync(token);
    }
}
