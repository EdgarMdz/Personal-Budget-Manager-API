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

        public Task<string> Login(UserDTO user)
        {
            throw new NotImplementedException();
        }

        public async Task<User> RegisterUser(UserDTO user, CancellationToken token)
        {
            var hashedPassword = _encryptionHashService.HashString(user.Password, out string salt);

            await _unitofWork.BeginTransactionAsync(token);
            try
            {
                if (
                    !int.TryParse(
                        await _encryptionHashService.DecryptAsync(user.RoleId, token),
                        out int userRoleId
                    )
                )
                    throw new Exception("Invalid RoleId");

                User newUser =
                    await _unitofWork
                        .GetRepository<User>()
                        .InsertAsync(
                            new()
                            {
                                Username = user.UserName,
                                PasswordHash = hashedPassword,
                                Salt = salt,
                                RoleId = userRoleId,
                            },
                            token
                        ) ?? throw new Exception("Failed to insert new user");

                await _unitofWork.CommitTransactionAsync(token);
                return newUser;
            }
            catch (OperationCanceledException e)
            {
                await _unitofWork.RollbackTransactionAsync(CancellationToken.None);
                throw new OperationCanceledException(
                    "User requested to cancel the operation. Rollback performed",
                    e,
                    token
                );
            }
            catch (Exception e)
            {
                await _unitofWork.RollbackTransactionAsync(CancellationToken.None);
                throw new Exception("An  exception occured", e);
            }
        }

        public async Task<IEnumerable<Models.UserRole>> GetUserRoleList(CancellationToken token)
        {
            var uiserRolesList = await _unitofWork
                .GetRepository<DataContext.Entities.UserRole>()
                .GetAllAsync(token);
            List<Models.UserRole> userRolesDto = [];

            foreach (var item in uiserRolesList)
            {
                var encryptedId = await _encryptionHashService.EncryptAsync(item.Id, token);
                userRolesDto.Add(new Models.UserRole() { Id = encryptedId, Name = item.Name });
            }

            return userRolesDto;
        }
    }
}
