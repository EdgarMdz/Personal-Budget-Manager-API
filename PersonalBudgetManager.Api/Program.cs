using Microsoft.AspNetCore.HttpsPolicy;

var builder = WebApplication.CreateBuilder(args);

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

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();

builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 443;
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
