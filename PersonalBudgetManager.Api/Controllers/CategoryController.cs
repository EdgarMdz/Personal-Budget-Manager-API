using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Common;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Models;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CategoryController(
        ILogger<CategoryController> logger,
        IUserService userService,
        ICategoryService categoriesService
    ) : BaseController(logger)
    {
        private ICategoryService _categoriesService = categoriesService;
        private IUserService _userService = userService;

        [HttpGet]
        [Authorize]
        [Route("GetAllCategories")]
        public async Task<IActionResult> GetUserCategories(CancellationToken token)
        {
            var userClaims = HttpContext.User;

            if (userClaims.Identity?.Name is not string userName)
                return BadRequest(ErrorMessages.InvalidToken);

            async Task<IActionResult> action()
            {
                if (await _userService.FindByName(userName, token) is not User user)
                    return BadRequest(ErrorMessages.InvalidUserCredentials);

                IEnumerable<CategoryDTO> categories = await _categoriesService.GetUserCategories(
                    user.Id,
                    token
                );
                return Ok(categories);
            }

            return await PerformActionSafely(action, null);
        }
    }
}
