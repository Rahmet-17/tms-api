using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwt(
        TmsUser user,
        IList<string> roles)
    {
        // Get JWT configuration

        var key = _config["Jwt:Key"];

        var issuer = _config["Jwt:Issuer"];

        var audience = _config["Jwt:Audience"];

        var expiryMinutes =
            _config["Jwt:ExpiryMinutes"];


        // Validate configuration

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT Key is missing from configuration.");
        }

        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT Issuer is missing from configuration.");
        }

        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT Audience is missing from configuration.");
        }


        // Claims

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            new Claim(
                ClaimTypes.Email,
                user.Email ?? string.Empty),

            new Claim(
                "FirstName",
                user.FirstName)
        };


        // Add roles
        foreach (var role in roles)
        {
            claims.Add(
                new Claim(
                    ClaimTypes.Role,
                    role));
        }


        // Signing key

        var securityKey =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));


        var credentials =
            new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);


        // Expiration

        var minutes = 15;

        if (!string.IsNullOrWhiteSpace(expiryMinutes))
        {
            minutes = int.Parse(expiryMinutes);
        }


        // Create JWT

        var token =
            new JwtSecurityToken(
                issuer: issuer,

                audience: audience,

                claims: claims,

                expires:
                    DateTime.UtcNow.AddMinutes(minutes),

                signingCredentials:
                    credentials);


        // Serialize token

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}