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
    ) : BaseController(logger, userService)
    {
        private readonly ICategoryService _categoriesService = categoriesService;
        private readonly IUserService _userService = userService;

        [HttpGet]
        [Authorize]
        [Route(ApiRoutes.GetAll)]
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

        [HttpPost]
        [Authorize]
        [Route(ApiRoutes.Create)]
        public async Task<IActionResult> RegisterNewCategory(
            CategoryDTO category,
            CancellationToken token
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                category = await _categoriesService.AddCategory(category, user.Id, token);
                return Ok(category);
            }
            return await PerformActionSafely(action, category);
        }

        [HttpPut]
        [Authorize]
        [Route(ApiRoutes.Modify)]
        public async Task<IActionResult> UpdateCategory(
            CategoryDTO category,
            CancellationToken token
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (category.Id < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                await _categoriesService.UpdateCategory(category, user.Id, token);
                return NoContent();
            }

            return await PerformActionSafely(action, category);
        }

        [HttpDelete]
        [Authorize]
        [Route(ApiRoutes.Delete)]
        public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (categoryId < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                await _categoriesService.DeleteCategory(categoryId, user.Id, token);

                return NoContent();
            }

            return await PerformActionSafely(action, categoryId);
        }

        [HttpGet]
        [Authorize]
        [Route(ApiRoutes.GetById)]
        public async Task<IActionResult> GetCategoryById(int categoryId, CancellationToken token)
        {
            if (categoryId < 0)
                return BadRequest(ErrorMessages.InvalidIdValue);

            async Task<IActionResult> action()
            {
                User user = await GetUser(HttpContext, token);
                CategoryDTO category = await _categoriesService.GetById(categoryId, user.Id, token);
                return Ok(category);
            }

            return await PerformActionSafely(action, categoryId);
        }
    }
}
