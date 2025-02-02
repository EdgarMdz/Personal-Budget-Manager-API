using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class IncomeController(IUserService userService, IIncomeService incomeService)
        : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly IIncomeService _incomeService = incomeService;

        [HttpGet]
        [Authorize]
        [Route("GetIncomes")]
        public async Task<IActionResult> GetUserIncomes(CancellationToken token)
        {
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string userName)
                return BadRequest("User name is null");

            if (await _userService.FindByName(userName, token) is not User user)
                return BadRequest("Operation not valid");

            var incomes = await _incomeService.GetIncomes(user.Id, token);
            return Ok(incomes);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
