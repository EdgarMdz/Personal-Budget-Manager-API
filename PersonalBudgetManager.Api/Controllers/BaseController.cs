using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    public class BaseController(ILogger logger, IUserService userService) : Controller
    {
        private readonly ILogger _logger = logger;
        private readonly IUserService _userService = userService;

        protected async Task<User> GetUser(HttpContext httpContext, CancellationToken token)
        {
            var userClaims = httpContext.User;
            if (userClaims.Identity?.Name is not string userName)
                throw new InvalidOperationException(ErrorMessages.InvalidToken);

            try
            {
                if (await _userService.FindByName(userName, token) is not User user)
                    throw new UnauthorizedAccessException(ErrorMessages.UnauthorizedOperation);

                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected async Task<IActionResult> PerformActionSafely(
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
                return NotFound(e.Message);
            }
            catch (DuplicateNameException e)
            {
                return Conflict(e.Message);
            }
            catch (Exception e)
            {
                var incomeJson = JsonSerializer.Serialize(parameters);
                var message = $"{e.Message}\nIncome details: {incomeJson}";

                _logger.LogError("Error at \"{MethodName}\": {Message}", methodName, message);

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
