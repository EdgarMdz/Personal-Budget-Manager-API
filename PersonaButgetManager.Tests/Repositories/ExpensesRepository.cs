using System.Globalization;
using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.DataContext.Entities;
using ExpenseAPIRepo = PersonalBudgetManager.Api.Repositories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class ExpensesRepository : BaseTest
    {
        [Fact]
        public async Task GetExpensesForUser_WhenUserExistAndHasExpenses_ReturnsEntities()
        {
            // Arrange
            int userid = 50;
            DateTime date = DateTime.UtcNow;
            var entities = await ResetDb<Expense>(100);

            // updating date
            var expense = entities.First(income => income.UserId == userid);
            expense.Date = date;

            await _dbcontext.SaveChangesAsync();

            var extraExpenses = Enumerable
                .Range(101, 5)
                .Select(i => new Expense()
                {
                    Id = i,
                    UserId = userid,
                    Date = date,
                    Amount = i,
                    Description = $"Expense {i}",
                    CategoryId = i,
                });
            await _dbcontext.Expenses.AddRangeAsync(extraExpenses);
            await _dbcontext.SaveChangesAsync();

            var expectedExpenses = extraExpenses.Append(expense);

            var token = CancellationToken.None;

            var repo = new ExpenseAPIRepo.ExpensesRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            // Act
            var result = await repo.GetExpensesForUser(userid, token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedExpenses.Count(), result.Count());
            Assert.All(
                result,
                actualExpense =>
                {
                    var expectedExpense = expectedExpenses.FirstOrDefault(e =>
                        e.Id == actualExpense.Id
                    );

                    Assert.NotNull(expectedExpense);
                    Assert.Equal(expectedExpense.UserId, actualExpense.UserId);
                    Assert.Equal(expectedExpense.Amount, actualExpense.Amount);
                    Assert.Equal(expectedExpense.Description, actualExpense.Description);
                    Assert.Equal(
                        expectedExpense.Date.ToString(
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture
                        ),
                        actualExpense.Date.ToString(
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture
                        )
                    );
                }
            );
        }

        [Fact]
        public async Task GetExpensesForUser_WhenUserExistAndHasNoExpenses_ReturnsEntities()
        {
            // Given
            await ResetDb<Expense>(100);

            var repo = new ExpenseAPIRepo.ExpensesRepository(
                _dbcontext,
                DelegatestrategyFactory.NoOpStrategy()
            );

            var token = CancellationToken.None;
            var userId = 400;

            // When
            var result = await repo.GetExpensesForUser(userId, token);

            // Then
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetExpensesForUser_WhenCanceledByTheUser_ThrowsOperationCanceledException()
        {
            // Arrange
            await ResetDb<Expense>(0);

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            var userId = 40;
            var repo = new ExpenseAPIRepo.ExpensesRepository(
                _dbcontext,
                DelegatestrategyFactory.DelayStrategy(5000)
            );

            // Act
            cancellationTokenSource.Cancel();

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await repo.GetExpensesForUser(userId, token)
            );
        }

        [Fact]
        public async Task GetExpensesForUser_WhenDbUpdateFails_ThrowsException()
        {
            // Arrange
            await ResetDb<Expense>(0);
            var token = CancellationToken.None;
            var userid = 30;
            var exceptionMessage = "Simulated Exception";
            var repo = new ExpenseAPIRepo.ExpensesRepository(
                _dbcontext,
                DelegatestrategyFactory.DbUpdateExceptionDelegate(exceptionMessage)
            );

            // Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.GetExpensesForUser(userid, token)
            );
            Assert.Contains(exceptionMessage, ex.Message);
        }

        [Fact]
        public async Task GetExpensesForUser_WhenGenericExceptionOccurs_ThrowsException()
        {
            // Arrange
            var exceptionMessage = "Simulated exception";
            var userId = 12;
            var token = CancellationToken.None;
            await ResetDb<Expense>(0);
            var repo = new ExpenseAPIRepo.ExpensesRepository(
                _dbcontext,
                DelegatestrategyFactory.ExceptionStrategy(exceptionMessage)
            );

            // Act and assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await repo.GetExpensesForUser(userId, token)
            );
            Assert.Contains(exceptionMessage, ex.Message);
        }
    }
}
