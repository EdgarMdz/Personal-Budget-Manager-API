using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;

namespace PersonalBudgetManager.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUser(UserDTO user, CancellationToken token);
        Task<string> Login(UserDTO user, CancellationToken token);
        Task<User?> FindByName(string userName, CancellationToken token);
        Category? FindCategory(User user, string category);
    }
}
