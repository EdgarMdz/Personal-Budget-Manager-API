namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }

        public virtual User? User { get; set; } //navigation property
        public virtual ICollection<Income> Incomes { get; set; } = []; //navigation property
        public virtual ICollection<Expense> Expenses { get; set; } = []; //navigation property
    }
}
