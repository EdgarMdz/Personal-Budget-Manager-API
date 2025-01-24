namespace PersonalBudgetManager.Api.DataContext.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Salt { get; set; }
    }
}
