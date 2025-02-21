using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.Common.Interfaces;

namespace PersonalBudgetManager.Api.Common
{
    public class DbUpdateExceptionThrower(string message) : IExceptionThrower
    {
        private readonly string _message = message;

        public void ThrowException()
        {
            throw new DbUpdateException(_message); // hay que inyectarlo a rapository y usarlo. luego hacer merge a
            // la rama de los test y continuar haciendo los tests ahi y usando este servicio para lanzar exceptions en los tests que ya habia
        }
    }
}
