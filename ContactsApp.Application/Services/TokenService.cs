using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContactsApp.Application.DTOs.Auth;
using ContactsApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ContactsApp.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string token, DateTime expiresAt) GenerateToken(UserTokenClaimsDto claims)
    {
        var jwtSection = _configuration.GetSection("JWT");
        var secret = jwtSection["Secret"]
            ?? throw new InvalidOperationException("JWT:Secret is not configured.");
        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("JWT:Issuer is not configured.");
        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("JWT:Audience is not configured.");

        var expirationMinutes = int.TryParse(jwtSection["ExpirationInMinutes"], out var minutes)
            ? minutes
            : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var jwtClaims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, claims.Id),
            new Claim(JwtRegisteredClaimNames.Email, claims.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, claims.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, claims.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: jwtClaims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return (token, expiresAt);
    }
}
