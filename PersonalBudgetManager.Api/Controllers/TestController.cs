using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class TestController(ILogger<TestController> logger, IUnitOfWork unitOfWork) : Controller
    {
        private readonly ILogger<TestController> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        [HttpGet]
        [Route("echo")]
        public IActionResult Echo(string message)
        {
            _logger.LogInformation("Echo message received: {Message}", message);
            return Ok(message);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
