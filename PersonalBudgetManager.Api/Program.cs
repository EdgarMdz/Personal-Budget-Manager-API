using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PersonalBudgetManager.Api.DataContext;
using PersonalBudgetManager.Api.Repositories;
using PersonalBudgetManager.Api.Repositories.Interfaces;
using PersonalBudgetManager.Api.Services;
using PersonalBudgetManager.Api.Services.Interfaces;

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

//configuring JWT bearer
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var secretKey =
            Environment.GetEnvironmentVariable("PersonalBudgetManager_SecretKey")
            ?? throw new InvalidOperationException(
                "PersonalBudgetManager_SecretKey not defined in environment variables."
            );

        var jwtConfigurations =
            builder.Configuration.GetSection("JWT")
            ?? throw new InvalidOperationException("Section JWT not found in appsettings.json");

        var issuer =
            jwtConfigurations["Issuer"]
            ?? throw new InvalidOperationException(
                "Issuer not defined at JWT section in appsettings.json"
            );

        var audience =
            jwtConfigurations["Audience"]
            ?? throw new InvalidOperationException(
                "Issuer not defined at JWT section in appsettings.json"
            );
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        };
    });

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddLogging();

// Adding dbcontext
builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        string connectionString =
            Environment.GetEnvironmentVariable("PersonalBudgetManager_ConnectionString")
            ?? throw new InvalidOperationException(
                "The connection string for the database is not configured."
            );

        options.UseLazyLoadingProxies().UseSqlServer(connectionString);
    },
    ServiceLifetime.Singleton
);

//Adding services
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<IIncomeRepository, IncomeRepository>();
builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IIncomeService, IncomeService>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();

var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
