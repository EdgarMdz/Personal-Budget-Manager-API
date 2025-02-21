using PersonalBudgetManager.Api.Common.Interfaces;

namespace PersonalBudgetManager.Api.Common
{
    public class GenericExceptionThrower(string message) : IExceptionThrower
    {
        private string _message = message;

        public void ThrowException()
        {
            throw new Exception(_message);
        }
    }
}
