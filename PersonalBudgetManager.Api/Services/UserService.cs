using System.Security.Authentication;
using PersonalBudgetManager.Api.Common;
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
    ) : BaseService(unitOfWork), IUserService
    {
        private readonly IJwtService _jwtService = jwtService;
        private readonly IEncryptionService _encryptionHashService = encryptionService;

        private readonly INameableRepository<User> _repo = unitOfWork.GetNameableRepository<User>();

        public async Task<User?> FindByName(string userName, CancellationToken token) =>
            await PerformTransactionalOperation(
                async () => await _repo.GetByNameAsync(userName, token),
                token
            );

        public async Task<string> Login(UserDTO user, CancellationToken token) =>
            await PerformTransactionalOperation(
                async () =>
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
                        throw new InvalidCredentialException(ErrorMessages.InvalidUserCredentials);

                    return _jwtService.GenerateToken(registeredUser);
                },
                token
            );

        public async Task<User> RegisterUser(UserDTO user, CancellationToken token)
        {
            var hashedPassword = _encryptionHashService.HashString(user.Password, out string salt);

            async Task<User> action()
            {
                User newUser =
                    await _repo.InsertAsync(
                        new()
                        {
                            Name = user.UserName,
                            PasswordHash = hashedPassword,
                            Salt = salt,
                        },
                        token
                    ) ?? throw new Exception(ErrorMessages.FailedToCreateUser);

                return newUser;
            }
            return await PerformTransactionalOperation(action, token);
        }
    }
}
