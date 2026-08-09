using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OPCBS.Application.Interfaces.Services;

namespace OPCBS.Infrastructure.Services;

/// <summary>
/// JWT token service implementation
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Guid userId, string email, string role, IEnumerable<string>? permissions = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["JwtSettings:Secret"] ?? "OPCBS-Default-Secret-Key-For-Dev-Only-Min32Chars!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("sub", userId.ToString())
        };

        if (permissions != null)
        {
            foreach (var perm in permissions)
                claims.Add(new Claim("permission", perm));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"] ?? "OPCBS",
            audience: _configuration["JwtSettings:Audience"] ?? "OPCBS",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                double.TryParse(_configuration["JwtSettings:ExpirationHours"], out var hours) ? hours : 1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Secret"] ?? "OPCBS-Default-Secret-Key-For-Dev-Only-Min32Chars!"));

            new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"] ?? "OPCBS",
                ValidateAudience = true,
                ValidAudience = _configuration["JwtSettings:Audience"] ?? "OPCBS",
                ValidateLifetime = true
            }, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
            return userId != null ? Guid.Parse(userId) : null;
        }
        catch
        {
            return null;
        }
    }
}
