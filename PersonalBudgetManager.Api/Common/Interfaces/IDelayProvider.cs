namespace PersonalBudgetManager.Api.Common.Interfaces
{
    public interface IDelayProvider
    {
        Task DelayAsync(TimeSpan delay, CancellationToken token);
    }
}
