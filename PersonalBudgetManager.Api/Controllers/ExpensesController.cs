using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class ExpensesController(ILogger<ExpensesController> logger) : BaseController(logger)
    {
        private readonly ILogger<ExpensesController> _logger = logger;

        [HttpGet]
        [Route("GetAllExpenses")]
        [Authorize]
        public Task<IActionResult> GetAllUserExpenses(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
