using Microsoft.AspNetCore.Mvc;
using PersonalBudgetManager.Api.Services;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(
        ILogger<CategoryController> logger,
        ICategoryService categoriesService
    ) : BaseController(logger)
    {
        private ICategoryService _categoriesService = categoriesService;
    }
}
