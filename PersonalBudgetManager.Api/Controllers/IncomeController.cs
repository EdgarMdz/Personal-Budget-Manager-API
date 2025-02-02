using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
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
                return BadRequest(ErrorMessages.InvalidToken);

            if (await _userService.FindByName(userName, token) is not User user)
                return BadRequest(ErrorMessages.UserNotFound);

            var incomes = await _incomeService.GetIncomes(user.Id, token);
            return Ok(incomes);
        }

        [HttpPost]
        [Authorize]
        [Route("RegisterIncome")]
        public async Task<IActionResult> RegisterIncome(IncomeDTO income, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userClaims = HttpContext.User;

                if (userClaims.Identity?.Name is not string username)
                    return BadRequest(ErrorMessages.UserNotFound);

                if (await _userService.FindByName(username, token) is not User user)
                    return BadRequest(ErrorMessages.InvalidToken);

                if (_userService.FindCategory(user, income.Category) is not Category category)
                    return BadRequest(ErrorMessages.NotRegisteredCategory);

                await _incomeService.AddIncome(income, category.Id, user.Id, token);

                return Ok("Income added to the user.");
            }
            catch (OperationCanceledException)
            {
                return StatusCode(449, ErrorMessages.OperationCanceled);
            }
            catch (Exception)
            {
                return StatusCode(500, ErrorMessages.UnexpectedError);
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
