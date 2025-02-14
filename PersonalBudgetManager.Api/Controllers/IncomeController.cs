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
        ILogger<IncomeController> _incomeLogger,
        IUserService userService,
        IIncomeService incomeService,
        ICategoryService categoryService
    ) : BaseController(_incomeLogger, userService)
    {
        private readonly IUserService _userService = userService;
        private readonly IIncomeService _incomeService = incomeService;
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet]
        [Authorize]
        [Route(ApiRoutes.GetAll)]
        public async Task<IActionResult> GetUserIncomes(CancellationToken token)
        {
            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                var incomes = await _incomeService.GetIncomes(user.Id, token);
                return Ok(incomes);
            }

            return await PerformActionSafely(action, null);
        }

        [HttpGet]
        [Authorize]
        [Route(ApiRoutes.GetById)]
        public async Task<IActionResult> GetUserIncomeById(int id, CancellationToken token)
        {
            if (id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            return await PerformActionSafely(
                async () =>
                {
                    User user = await GetUser(HttpContext, token);

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
        [Route(ApiRoutes.Create)]
        public async Task<IActionResult> RegisterIncome(IncomeDTO income, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await PerformActionSafely(
                async () =>
                {
                    User user = await GetUser(HttpContext, token);

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
        [Route(ApiRoutes.Modify)]
        public async Task<IActionResult> UpdateIncome(IncomeDTO income, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (income.Id == null)
                return BadRequest($"{ErrorMessages.ProvideParater}: {nameof(income.Id)}");

            return await PerformActionSafely(
                async () =>
                {
                    User user = await GetUser(HttpContext, token);
                    await _incomeService.UpdateIncome(income, user.Id, token);

                    return NoContent();
                },
                income
            );
        }

        [HttpDelete]
        [Authorize]
        [Route(ApiRoutes.Delete)]
        public async Task<IActionResult> DeleteIncome(int id, CancellationToken token)
        {
            if (id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                await _incomeService.DeleteIncome(id, user.Id, token);
                return NoContent();
            }
            return await PerformActionSafely(action, id);
        }
    }
}
