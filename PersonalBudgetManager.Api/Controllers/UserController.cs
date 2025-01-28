using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [Route("[controller]")]
    public class UserController(ILogger<UserController> logger, IUnitOfWork unitOfWork) : Controller
    {
        private readonly ILogger<UserController> _logger = logger;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly Repository<User> _userRepo = unitOfWork.GetRepository<User>();

        [HttpPost]
        [Route("RegisterUser")]
        public IActionResult CreateUser(UserDTO user)
        {
            return Ok(user);
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
