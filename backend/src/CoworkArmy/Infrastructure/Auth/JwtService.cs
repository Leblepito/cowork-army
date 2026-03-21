using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CoworkArmy.Infrastructure.Auth;

public class JwtService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtService(IConfiguration config)
    {
        _secret = config["JWT_SECRET"]
            ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? "cowork-army-dev-secret-key-minimum-32-chars!!";

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")
            ?? "Development";

        if (env == "Production")
        {
            if (_secret.Length < 32)
                throw new InvalidOperationException(
                    "JWT_SECRET must be at least 32 characters in Production.");
            if (_secret.Contains("default") || _secret.Contains("dev") || _secret.Contains("secret"))
                throw new InvalidOperationException(
                    "JWT_SECRET appears to be a development value. Set a strong production secret.");
        }

        _issuer = config["JWT_ISSUER"] ?? "cowork.army";
        _audience = config["JWT_AUDIENCE"] ?? "cowork.army";
        _expiryMinutes = int.TryParse(config["JWT_EXPIRY_MINUTES"], out var m) ? m : 480; // 8 hours default
    }

    public string GenerateToken(string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public SymmetricSecurityKey GetSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));

    public string Issuer => _issuer;
    public string Audience => _audience;
}
