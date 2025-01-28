using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class UserRole : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public virtual ICollection<User> Users { get; set; } = []; //navigation property
    }
}
