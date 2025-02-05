using Microsoft.AspNetCore.Mvc;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class ExpensesController(ILogger<ExpensesController> logger) : BaseController(logger)
    {
        private readonly ILogger<ExpensesController> _logger = logger;
    }
}
