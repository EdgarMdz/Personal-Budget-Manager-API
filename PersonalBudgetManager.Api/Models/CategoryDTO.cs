using System.ComponentModel.DataAnnotations;

namespace PersonalBudgetManager.Api.Models
{
    public class CategoryDTO
    {
        public int Id { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}
