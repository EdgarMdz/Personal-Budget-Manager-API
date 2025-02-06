using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class ExpensesController(
        ILogger<ExpensesController> logger,
        IUserService userService,
        IExpensesService expensesService,
        ICategoryService categoryService
    ) : BaseController(logger)
    {
        private readonly IUserService _userService = userService;
        private readonly IExpensesService _expensesService = expensesService;
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet]
        [Authorize]
        [Route("GetAllExpenses")]
        public async Task<IActionResult> GetUserExpenses(CancellationToken token)
        {
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string userName)
                return BadRequest(ErrorMessages.InvalidToken);

            async Task<IActionResult> action()
            {
                if (await _userService.FindByName(userName, token) is not User user)
                    return BadRequest(ErrorMessages.UserNotFound);

                var expenses = await _expensesService.GetExpenses(user.Id, token);
                return Ok(expenses);
            }

            return await PerformActionSafely(action, null);
        }

        [HttpGet]
        [Authorize]
        [Route("GetExpense")]
        public async Task<IActionResult> GetUserExpenseById(int id, CancellationToken token)
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

                    ExpenseDTO expense = await _expensesService.GetExpenseById(id, user.Id, token);

                    return Ok(expense);
                },
                id
            );
        }

        /// <summary>
        /// Registers a new expense for the authenticated user.
        /// </summary>
        /// <param name="expense">The expense data transfer object containing the details of the expense to be registered.</param>
        /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// <response code="200">Expense added to the user.</response>
        /// <response code="400">Bad request if the model state is invalid, user is not found, token is invalid, or category is not registered.</response>
        /// <response code="449">Operation canceled.</response>
        /// <response code="500">An unexpected error occurred.</response>
        [HttpPost]
        [Authorize]
        [Route("RegisterExpense")]
        public async Task<IActionResult> RegisterExpense(
            ExpenseDTO expense,
            CancellationToken token
        )
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
                        await _categoryService.GetUserCategory(user.Id, expense.Category, token)
                        is not Category category
                    )
                        return BadRequest(ErrorMessages.NotRegisteredCategory);

                    var newExpense = await _expensesService.AddExpense(
                        expense,
                        category.Id,
                        user.Id,
                        token
                    );

                    return CreatedAtAction(
                        nameof(GetUserExpenseById),
                        new { id = newExpense.Id },
                        newExpense
                    );
                },
                expense
            );
        }

        [HttpPut]
        [Authorize]
        [Route("UpdateExpense")]
        public async Task<IActionResult> UpdateExpense(ExpenseDTO expense, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (expense.Id == null)
                return BadRequest($"{ErrorMessages.ProvideParater}: {nameof(expense.Id)}");

            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.UserNotFound);

            return await PerformActionSafely(
                async () =>
                {
                    if (await _userService.FindByName(username, token) is not User user)
                        return BadRequest(ErrorMessages.UserNotFound);

                    await _expensesService.UpdateExpense(expense, user.Id, token);

                    return NoContent();
                },
                expense
            );
        }

        [HttpDelete]
        [Authorize]
        [Route("DeleteExpense")]
        public async Task<IActionResult> DeleteExpense(int id, CancellationToken token)
        {
            if (id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string username)
                return BadRequest(ErrorMessages.UserNotFound);

            async Task<IActionResult> action()
            {
                if (await _userService.FindByName(username, token) is not User user)
                    return BadRequest(ErrorMessages.UserNotFound);

                await _expensesService.DeleteExpense(id, user.Id, token);
                return NoContent();
            }
            return await PerformActionSafely(action, id);
        }
    }
}
