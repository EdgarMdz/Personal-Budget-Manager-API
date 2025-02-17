using PersonalBudgetManager.Api.Common.Interfaces;

namespace PersonalBudgetManager.Api.Common
{
    public class NoDelayProvider : IDelayProvider
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken token) => Task.CompletedTask;
    }
}
