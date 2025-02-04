using System.Net;
using Microsoft.EntityFrameworkCore.Storage;
using PersonalBudgetManager.Api.Repositories.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class BaseService(IUnitOfWork unitOfWork)
    {
        protected readonly IUnitOfWork _unitOfWork = unitOfWork;

        protected async Task<T> PerformTransactionalOperation<T>(
            Func<Task<T>> action,
            CancellationToken token
        )
        {
            IDbContextTransaction? transaction = null;

            try
            {
                transaction = await _unitOfWork.BeginTransactionAsync(token);
                var result = await action();
                await _unitOfWork.SaveChangesAsync(token);
                await _unitOfWork.CommitTransactionAsync(token);
                return result;
            }
            catch (Exception)
            {
                if (transaction != null)
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);

                throw;
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
            }
        }
    }
}
