namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class Expense
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public int? CategoryId { get; set; }
        public required string Description { get; set; }
        public DateTime Date { get; set; }

        public virtual required User User { get; set; }
        public virtual Category? Category { get; set; }
    }
}
