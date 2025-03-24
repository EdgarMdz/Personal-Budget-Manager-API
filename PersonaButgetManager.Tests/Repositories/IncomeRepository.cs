using System.Globalization;
using PersonaButgetManager.Tests.Common.Factories;
using PersonalBudgetManager.Api.DataContext.Entities;
using IncomeAPIRepo = PersonalBudgetManager.Api.Repositories.IncomeRepository;

namespace PersonaButgetManager.Tests.Repositories
{
    public class IncomeRepository : BaseTest
    {
        [Fact]
        public async Task GetIncomesForUser_WhenUserHasIncomes_ReturnsThem()
        {
            // Arrange
            var repo = new IncomeAPIRepo(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            var incomes = await ResetDb<Income>(100);

            var token = CancellationToken.None;
            var userId = 50;
            var date = DateTime.UtcNow;

            var income = incomes.First(income => income.UserId == userId);
            income.Date = date;

            await _dbcontext.SaveChangesAsync();

            var extraIncomes = Enumerable
                .Range(101, 5)
                .Select(i => new Income()
                {
                    Id = i,
                    UserId = userId,
                    Date = date,
                    Amount = i,
                    Description = $"Income {i}",
                    CategoryId = i,
                });
            await _dbcontext.Incomes.AddRangeAsync(extraIncomes);
            await _dbcontext.SaveChangesAsync();

            var expectedIncomes = extraIncomes.Append(income);

            // Act
            var result = await repo.GetIncomesForUser(userId, token);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(expectedIncomes.Count(), result.Count());
            Assert.All(
                result,
                actualIncome =>
                {
                    var expectedIncome = expectedIncomes.FirstOrDefault(income =>
                        income.Id == actualIncome.Id
                    );

                    Assert.NotNull(expectedIncome);
                    Assert.Equal(expectedIncome.UserId, actualIncome.UserId);
                    Assert.Equal(expectedIncome.Amount, actualIncome.Amount);
                    Assert.Equal(expectedIncome.Description, actualIncome.Description);
                    Assert.Equal(expectedIncome.CategoryId, actualIncome.CategoryId);
                    Assert.Equal(
                        expectedIncome.Date.ToString(
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture
                        ),
                        actualIncome.Date.ToString(
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture
                        )
                    );
                }
            );
        }

        [Fact]
        public async Task GetIncomesForUser_WhenUserHasNoIncomes_ReturnsEmpty()
        {
            // Arrange
            await ResetDb<Income>(100);
            var repo = new IncomeAPIRepo(_dbcontext, DelegatestrategyFactory.NoOpStrategy());

            var userId = 200;
            var token = CancellationToken.None;
            // Act
            var result = await repo.GetIncomesForUser(userId, token);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
