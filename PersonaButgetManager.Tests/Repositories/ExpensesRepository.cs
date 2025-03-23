using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.DataContext.Entities;
using ExpenseAPIRepo = PersonalBudgetManager.Api.Repositories;

namespace PersonaButgetManager.Tests.Repositories
{
    public class ExpensesRepository : BaseTest
    {
        [Fact]
        public async Task GetExpensesForUser_WhenUserExistAndHaveRecords_ReturnsEntities()
        {
            // Arrange
            int userid = 50;
            DateTime date = DateTime.UtcNow;
            var entities = await ResetDb<Expense>(100);

            // Actualizar la fecha de un gasto existente
            var ent = entities.First(e => e.UserId == userid);
            ent.Date = date;
            await _dbcontext.SaveChangesAsync();

            // Crear nuevos gastos
            var extraEntities = Enumerable
                .Range(101, 5)
                .Select(i => new Expense()
                {
                    Id = i,
                    UserId = userid,
                    Date = date,
                    Amount = i,
                    Description = $"Expense test {i}",
                    CategoryId = i,
                });

            await _dbcontext.Expenses.AddRangeAsync(extraEntities);
            await _dbcontext.SaveChangesAsync();

            var expectedExpenses = extraEntities.Append(ent);

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

                    // Comparación de fechas segura
                    Assert.Equal(
                        expectedExpense.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                        actualExpense.Date.ToString("yyyy-MM-dd HH:mm:ss")
                    );
                }
            );
        }
    }
}
