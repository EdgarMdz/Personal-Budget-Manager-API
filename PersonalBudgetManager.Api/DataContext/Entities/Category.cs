using Microsoft.Identity.Client;

namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }

        public virtual required User User { get; set; } //navigation property
    }
}
