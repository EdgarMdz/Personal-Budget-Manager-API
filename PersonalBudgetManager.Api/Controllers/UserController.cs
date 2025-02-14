using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class UserController(ILogger<UserController> logger, IUserService userService)
        : BaseController(logger, userService)
    {
        private readonly IUserService _userService = userService;

        [HttpPost]
        [Route(ApiRoutes.Create)]
        public async Task<IActionResult> CreateUser(UserDTO user, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("password cannot be empty or whitespaces");

            if (user.Password.Length < 8)
                return BadRequest("the password must have a minimum of 8 characters");

            bool hasNumbers = user.Password.Any(char.IsDigit);
            bool hasCapitals = user.Password.Any(char.IsUpper);
            bool hasSpecialCharacters = user.Password.Any(c => !char.IsLetterOrDigit(c));

            if (!hasNumbers || !hasCapitals || !hasSpecialCharacters)
                return BadRequest(
                    "The password must contains at least one number, one capital letter and one spacial character"
                );

            if (user.UserName.Trim() == string.Empty)
                return BadRequest("Username cannot be empty or whitespaces");

            user.UserName = user.UserName.Trim();

            async Task<IActionResult> action()
            {
                if (await _userService.FindByName(user.UserName, token) != null)
                    return BadRequest("Choose a different user name");

                var newUser = await _userService.RegisterUser(user, token);
                return Ok("User successfully registered. :)");
            }

            return await PerformActionSafely(action, user);
        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login(UserDTO user, CancellationToken token)
        {
            if (string.IsNullOrEmpty(user.UserName))
                return BadRequest("Provide a user name");

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Provide a password");

            async Task<IActionResult> action()
            {
                user.UserName = user.UserName.Trim();
                return Ok(await _userService.Login(user, token));
            }

            return await PerformActionSafely(action, user);
        }
    }
}
