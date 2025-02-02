using System.ComponentModel.DataAnnotations;

namespace PersonalBudgetManager.Api.Models
{
    public class IncomeDTO
    {
        [Range(
            0.01,
            double.MaxValue,
            ErrorMessage = "The income must be a positive value greater than 0"
        )]
        public required decimal Amount { get; set; }

        [Required]
        public required string Category { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
