namespace PersonalBudgetManager.Api.Models
{
    public class IncomeDTO
    {
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public required string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
