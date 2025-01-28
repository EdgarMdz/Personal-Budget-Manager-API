using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();

// Configure Kestrel to listen on both HTTP and HTTPS ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80); // HTTP port
    options.ListenAnyIP(
        443,
        listenOptions =>
        {
            var certificatePath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources",
                "Certificates",
                "PersonalBudgetManager.pfx"
            );
            var pass =
                Environment.GetEnvironmentVariable("PersonalBudgetManagerCertPass")
                ?? throw new InvalidOperationException(
                    "The environment variable 'PersonalBudgetManagerCertPass' is not set."
                );

            listenOptions.UseHttps(certificatePath, pass); // HTTPS port with certificate
        }
    );
});

//settiing https port to redirect http requests
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 443;
});

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

//Adding unit of work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
