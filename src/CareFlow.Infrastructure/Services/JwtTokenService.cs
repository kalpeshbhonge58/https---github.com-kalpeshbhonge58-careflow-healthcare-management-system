using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CareFlow.Application.Interfaces;
using CareFlow.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CareFlow.Infrastructure.Services;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "CareFlow";
    public string Audience { get; set; } = "CareFlowUsers";
    public int ExpirationMinutes { get; set; } = 60;
}

public class JwtTokenService : IJwtTokenService, ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IConfiguration configuration)
    {
        _settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(_settings.Secret))
            _settings.Secret = "CareFlow-Super-Secret-Key-For-Development-Only-2026!";
    }

    string IJwtTokenService.GenerateToken(User user, IEnumerable<string> roles) =>
        GenerateTokenInternal(user, roles).Token;

    (string Token, DateTime ExpiresAt) ITokenService.GenerateToken(User user, IEnumerable<string> roles) =>
        GenerateTokenInternal(user, roles);

    public DateTime GetTokenExpiration() =>
        DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

    private (string Token, DateTime ExpiresAt) GenerateTokenInternal(User user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = GetTokenExpiration();

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}
