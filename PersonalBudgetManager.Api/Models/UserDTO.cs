namespace PersonalBudgetManager.Api.Models
{
    public class UserDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public required UserRole UserRole { get; set; }

        public required string Id { get; set; }
    }
}
