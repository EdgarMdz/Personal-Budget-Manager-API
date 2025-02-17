using PersonalBudgetManager.Api.Common.Interfaces;

namespace PersonalBudgetManager.Api.Common
{
    public class RealDelayProvider : IDelayProvider
    {
        public Task DelayAsync(TimeSpan delay, CancellationToken token) => Task.Delay(delay, token);
    }
}
