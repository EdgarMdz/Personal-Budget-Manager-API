using System.Security.Authentication;
using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class UserService(
        IJwtService jwtService,
        IEncryptionService encryptionService,
        IUnitOfWork unitOfWork
    ) : IUserService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEncryptionService _encryptionHashService = encryptionService;
        private readonly IUnitOfWork _unitofWork = unitOfWork;

        private readonly INameableRepository<User> _repo = unitOfWork.GetNameableRepository<User>();

        public async Task<User?> FindByName(string userName, CancellationToken token) =>
            await _repo.GetByNameAsync(userName, token);

        public Category? FindCategory(User user, string category) =>
            user.Categories.FirstOrDefault(cat =>
                cat.Name.Equals(category, StringComparison.CurrentCultureIgnoreCase)
            );

        public async Task<string> Login(UserDTO user, CancellationToken token)
        {
            try
            {
                var registeredUser = await _repo.GetByNameAsync(user.UserName, token);

                if (
                    registeredUser is null
                    || !_encryptionHashService.CompareHashStrings(
                        user.Password,
                        registeredUser.PasswordHash,
                        registeredUser.Salt
                    )
                )
                    throw new InvalidCredentialException("User name or password are not valid. :/");

                return _jwtService.GenerateToken(registeredUser);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<User> RegisterUser(UserDTO user, CancellationToken token)
        {
            var hashedPassword = _encryptionHashService.HashString(user.Password, out string salt);

            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await _unitofWork.BeginTransactionAsync(token);
                User newUser =
                    await _repo.InsertAsync(
                        new()
                        {
                            Name = user.UserName,
                            PasswordHash = hashedPassword,
                            Salt = salt,
                        },
                        token
                    ) ?? throw new Exception("Failed to insert new user");

                await _unitofWork.SaveChangesAsync(token);
                await _unitofWork.CommitTransactionAsync(token);
                return newUser;
            }
            catch (Exception)
            {
                if (transaction != null)
                    await _unitofWork.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
        }
    }
}
