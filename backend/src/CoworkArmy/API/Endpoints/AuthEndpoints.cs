using System.Collections.Concurrent;
using CoworkArmy.Infrastructure.Auth;

namespace CoworkArmy.API.Endpoints;

public static class AuthEndpoints
{
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _loginAttempts = new();
    private const int MaxAttempts = 5;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", (HttpContext ctx, LoginRequest req, JwtService jwt, IConfiguration config) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Rate limit check
            if (_loginAttempts.TryGetValue(ip, out var entry))
            {
                if (DateTime.UtcNow - entry.WindowStart < Window && entry.Count >= MaxAttempts)
                    return Results.Json(new { error = "Too many login attempts. Try again later." },
                        statusCode: 429);
                if (DateTime.UtcNow - entry.WindowStart >= Window)
                    _loginAttempts[ip] = (0, DateTime.UtcNow);
            }

            var adminEmail = config["ADMIN_EMAIL"]
                ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL")
                ?? "admin@cowork.army";
            var adminPass = config["ADMIN_PASSWORD"]
                ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
                ?? "cowork-admin-dev";

            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") ?? "Development";
            if (env == "Production" && adminPass.Contains("admin"))
                return Results.Problem("ADMIN_PASSWORD must be set in production");

            if (!string.Equals(req.Email, adminEmail, StringComparison.OrdinalIgnoreCase)
                || req.Password != adminPass)
            {
                // Record failed attempt
                _loginAttempts.AddOrUpdate(ip,
                    _ => (1, DateTime.UtcNow),
                    (_, old) => (old.Count + 1, old.WindowStart));
                return Results.Json(new { error = "Invalid credentials" }, statusCode: 401);
            }

            // Reset on success
            _loginAttempts.TryRemove(ip, out _);

            var token = jwt.GenerateToken(req.Email, "admin");
            return Results.Ok(new { token, email = req.Email, role = "admin", expiresIn = 480 * 60 });
        }).WithTags("Auth").AllowAnonymous();
    }
}

public record LoginRequest(string Email, string Password);
