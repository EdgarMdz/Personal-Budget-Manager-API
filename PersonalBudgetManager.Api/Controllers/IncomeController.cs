using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class IncomeController(
        ILogger<IncomeController> logger,
        IUserService userService,
        IIncomeService incomeService
    ) : Controller
    {
        private readonly ILogger<IncomeController> _logger = logger;
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

        /// <summary>
        /// Registers a new income for the authenticated user.
        /// </summary>
        /// <param name="income">The income data transfer object containing the details of the income to be registered.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// <response code="200">Income added to the user.</response>
        /// <response code="400">Bad request if the model state is invalid, user is not found, token is invalid, or category is not registered.</response>
        /// <response code="449">Operation canceled.</response>
        /// <response code="500">An unexpected error occurred.</response>
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
            catch (Exception e)
            {
                _logger.LogError(
                    "An unexpected error occurred: {Message}\nIncome details: {@Income}",
                    e.Message,
                    income
                );

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
