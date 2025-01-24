using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();

// Adding dbcontext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectionString =
        Environment.GetEnvironmentVariable("PersonalBudgetManager_ConnectionString")
        ?? throw new InvalidOperationException(
            "The connection string for the database is not configured."
        );
    options.UseLazyLoadingProxies().UseSqlServer(connectionString);
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
