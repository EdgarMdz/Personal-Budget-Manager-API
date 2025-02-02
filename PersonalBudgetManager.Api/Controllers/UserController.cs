using System.Security.Authentication;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class UserController(ILogger<UserController> logger, IUserService userService)
        : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUserService _userService = userService;

        [HttpPost]
        [Route("RegisterUser")]
        public async Task<IActionResult> CreateUser(UserDTO user, CancellationToken token)
        {
            if (user.UserName.Trim() == string.Empty)
                return BadRequest("Username cannot be empty or whitespaces");

            user.UserName = user.UserName.Trim();
            try
            {
                if (await _userService.FindByName(user.UserName, token) != null)
                    return BadRequest("Choose a different user name");

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

                var newUser = await _userService.RegisterUser(user, token);
                return Ok("User successfully registered. :)");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(449, "The request was canceled by the user");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected server error occurred. :(");
            }
        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login(UserDTO user, CancellationToken token)
        {
            if (string.IsNullOrEmpty(user.UserName))
                return BadRequest("Provide a user name");

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Provide a password");

            try
            {
                user.UserName = user.UserName.Trim();
                return Ok(await _userService.Login(user, token));
            }
            catch (InvalidCredentialException e)
            {
                return BadRequest(e.Message);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(449, "Operation canceled by the user");
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected server error occurred. :(");
            }
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
