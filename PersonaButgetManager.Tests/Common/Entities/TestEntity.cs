using PersonalBudgetManager.Api.DataContext.Interfaces;

namespace PersonaButgetManager.Tests.Common.Entities
{
    public class TestEntity : IEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
