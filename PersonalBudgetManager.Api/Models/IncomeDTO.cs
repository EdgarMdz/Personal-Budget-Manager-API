using System.ComponentModel.DataAnnotations;
using PersonalBudgetManager.Api.Common;

namespace PersonalBudgetManager.Api.Models
{
    public class IncomeDTO
    {
        [Range(0.01, double.MaxValue, ErrorMessage = ErrorMessages.InvalidIncomeValue)]
        public required decimal Amount { get; set; }

        [Required]
        public required string Category { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
