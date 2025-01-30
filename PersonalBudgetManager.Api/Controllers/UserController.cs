using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.DataContext.Entities;
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

        [HttpGet]
        [Route("GetUserRoles")]
        public async Task<IActionResult> GetUserRolesList()
        {
            var roles = await _userService.GetUserRoleList(CancellationToken.None);
            return Ok(roles);
        }

        [HttpPost]
        [Route("RegisterUser")]
        public IActionResult CreateUser(UserDTO user, CancellationToken token)
        {
            if (user.UserName.Trim() == string.Empty)
                return BadRequest("Username cannot be empty or whitespaces");
            if (user.Password.Trim() == string.Empty)
                return BadRequest("password cannot be empty or whitespaces");

            if (user.Password.Trim().Length < 8)
                return BadRequest("the password must have a minimum of 8 characters");

            bool hasNumbers = false;
            bool hasCapitals = false;
            bool hasSpecialCharacters = false;
            foreach (char c in user.Password)
            {
                if (char.IsDigit(c))
                    hasNumbers = true;
                if (char.IsLetter(c) && char.IsUpper(c))
                    hasCapitals = true;

                if (!char.IsLetter(c) && !char.IsDigit(c))
                    hasSpecialCharacters = true;

                if (hasCapitals && hasNumbers && hasSpecialCharacters)
                    break;
            }

            if (!hasNumbers || !hasCapitals || !hasSpecialCharacters)
                return BadRequest(
                    "The password must contains at least one number, one capital letter and one spacial character"
                );

            _userService.RegisterUser(user, token);
            return Ok();
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
