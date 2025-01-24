using Microsoft.AspNetCore.Mvc;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class TestController(ILogger<TestController> logger) : Controller
    {
        private readonly ILogger<TestController> _logger = logger;

        [HttpGet]
        [Route("echo")]
        public IActionResult Echo(string message)
        {
            _logger.LogInformation(message);
            return Ok(message);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
