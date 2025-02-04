using System.Runtime.CompilerServices;
using System.Text.Json;
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
        IIncomeService incomeService,
        ICategoryService categoryService
    ) : Controller
    {
        private readonly ILogger<IncomeController> _logger = logger;
        private readonly IUserService _userService = userService;
        private readonly IIncomeService _incomeService = incomeService;
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet]
        [Authorize]
        [Route("GetAllIncomes")]
        public async Task<IActionResult> GetUserIncomes(CancellationToken token)
        {
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string userName)
                return BadRequest(ErrorMessages.InvalidToken);
            try
            {
                if (await _userService.FindByName(userName, token) is not User user)
                    return BadRequest(ErrorMessages.UserNotFound);

                var incomes = await _incomeService.GetIncomes(user.Id, token);
                return Ok(incomes);
            }
            catch (OperationCanceledException)
            {
                return StatusCode(449, ErrorMessages.OperationCanceled);
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Error at {MethodName}: {Message}",
                    nameof(GetUserIncomes),
                    e.Message
                );
                return StatusCode(500, ErrorMessages.UnexpectedError);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("GetIncome")]
        public async Task<IActionResult> GetUserIncomeById(int id, CancellationToken token)
        {
            if (id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.InvalidIdValue);

            return await PerformActionSafely(
                async () =>
                {
                    if (await _userService.FindByName(username, token) is not User user)
                        return BadRequest(ErrorMessages.InvalidIdValue);

                    IncomeDTO income = await _incomeService.GetIncomeById(id, user.Id, token);

                    return Ok(income);
                },
                id
            );
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
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.UserNotFound);

            return await PerformActionSafely(
                async () =>
                {
                    if (await _userService.FindByName(username, token) is not User user)
                        return BadRequest(ErrorMessages.UserNotFound);

                    if (
                        await _categoryService.GetUserCategory(user.Id, income.Category, token)
                        is not Category category
                    )
                        return BadRequest(ErrorMessages.NotRegisteredCategory);

                    var newIncome = await _incomeService.AddIncome(
                        income,
                        category.Id,
                        user.Id,
                        token
                    );

                    return CreatedAtAction(
                        nameof(GetUserIncomes),
                        new { id = newIncome.Id },
                        newIncome
                    );
                },
                income
            );
        }

        [HttpPut]
        [Authorize]
        [Route("UpdateIncome")]
        public async Task<IActionResult> UpdateIncome(IncomeDTO income, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (income.Id == null)
                return BadRequest($"{ErrorMessages.ProvideParater}: {nameof(income.Id)}");
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.UserNotFound);

            return await PerformActionSafely(
                async () =>
                {
                    if (await _userService.FindByName(username, token) is not User user)
                        return BadRequest(ErrorMessages.UserNotFound);

                    await _incomeService.UpdateIncome(income, user.Id, token);

                    return NoContent();
                },
                income
            );
        }

        private async Task<IActionResult> PerformActionSafely(
            Func<Task<IActionResult>> action,
            object? parameters,
            [CallerMemberName] string methodName = ""
        )
        {
            try
            {
                return await action();
            }
            catch (OperationCanceledException)
            {
                return StatusCode(449, ErrorMessages.OperationCanceled);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                var incomeJson = JsonSerializer.Serialize(parameters);
                var message = $"{e.Message}\nIncome details: {incomeJson}";

                _logger.LogError("Error at \"{MethodName}\": {Message}", methodName, message);

                return StatusCode(500, ErrorMessages.UnexpectedError);
            }
        }

        [HttpDelete]
        [Authorize]
        [Route("DeleteIncome")]
        public async Task<IActionResult> DeleteIncome(int id, CancellationToken token)
        {
            if (id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.UserNotFound);

            return await PerformActionSafely(
                async () =>
                {
                    if (await _userService.FindByName(username, token) is not User user)
                        return BadRequest(ErrorMessages.UserNotFound);

                    await _incomeService.DeleteIncome(id, user.Id, token);
                    return NoContent();
                },
                id
            );
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
