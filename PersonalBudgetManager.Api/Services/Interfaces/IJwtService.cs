using PersonalBudgetManager.Api.DataContext.Entities;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IJwtService
    {
        public string GenerateToken(User user);
    }
}
