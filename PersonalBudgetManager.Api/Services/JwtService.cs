using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PersonalBudgetManager.Api.DataContext.Entities;
using PersonalBudgetManager.Api.Services.Interfaces;

namespace PersonalBudgetManager.Api.Services
{
    public class JwtService(IConfiguration configuration) : IJwtService
    {
        private readonly IConfiguration _configuration = configuration;

        public string GenerateToken(User user)
        {
            var secretKey =
                Environment.GetEnvironmentVariable("PersonalBudgetManager_SecretKey")
                ?? throw new InvalidOperationException(
                    "PersonalBudgetManager_SecretKey not defined in environment variables."
                );

            var jwtConfigurations =
                _configuration.GetSection("JWT")
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

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.Name),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
