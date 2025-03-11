using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common;

namespace PersonaButgetManager.Tests.Common.Factories
{
    public class DelegatestrategyFactory
    {
        public static DelegateStrategy NoOpStrategy() => new((ct) => Task.CompletedTask);

        public static DelegateStrategy DelayStrategy(int miliSeconds) =>
            new((ct) => Task.Delay(miliSeconds, ct));

        public static DelegateStrategy ExceptionStrategy(string message) =>
            new((ct) => throw new Exception(message));

        public static DelegateStrategy DbUpdateExceptionDelegate(string message) =>
            new((ct) => throw new DbUpdateException(message));
    }
}
