namespace PersonalBudgetManager.Api.Common.Interfaces
{
    public interface IStrategy
    {
        Task PerformTask(CancellationToken token);
    }
}
