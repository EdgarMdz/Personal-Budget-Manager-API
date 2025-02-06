using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;

namespace PersonalBudgetManager.Api.Controllers
{
    public class BaseController(ILogger logger) : Controller
    {
        private readonly ILogger _logger = logger;

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
