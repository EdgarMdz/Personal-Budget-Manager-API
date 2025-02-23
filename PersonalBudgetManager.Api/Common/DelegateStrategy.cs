using PersonalBudgetManager.Api.Common.Interfaces;

namespace PersonalBudgetManager.Api.Common
{
    public class DelegateStrategy(Func<CancellationToken, Task> action) : IStrategy
    {
        private readonly Func<CancellationToken, Task> _action = action;

        public async Task PerformTask(CancellationToken token) => await _action(token);
    }
}
