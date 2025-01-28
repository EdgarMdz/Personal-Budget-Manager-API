using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class User : IEntity
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Salt { get; set; }
        public required int RoleId { get; set; }
        public virtual required UserRole Role { get; set; } // navigation property
        public virtual ICollection<Category> Categories { get; set; } = []; //navigation property

        public virtual ICollection<Income> Incomes { get; set; } = []; //navigation property

        public virtual ICollection<Expense> Expenses { get; set; } = []; //navigation property
    }
}
