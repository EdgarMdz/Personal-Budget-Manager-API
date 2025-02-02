using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class IncomeController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        [Authorize]
        [Route("GetIncomes")]
        public Task<IActionResult> GetUserIncomes()
        {
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string userName)
                return Task.FromResult<IActionResult>(BadRequest("User name is null"));

            return Task.FromResult<IActionResult>(Ok($"Your user name is {userName}. ^^"));
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
