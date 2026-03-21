using CoworkArmy.Domain.Agents;
using CoworkArmy.Infrastructure.Persistence;

namespace CoworkArmy.API.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> AuditPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/auth/login", "/api/hr/spawn", "/api/hr/retire",
        "/api/hr/warn", "/api/hr/proposals"
    };

    public AuditMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Request.Method != "POST") return;

        var path = context.Request.Path.Value ?? "";
        var shouldAudit = AuditPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        if (!shouldAudit) return;

        try
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CoworkDbContext>();
            db.AuditLogs.Add(new AuditLog
            {
                Action = $"{context.Request.Method} {path}",
                UserId = context.User?.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Details = $"Status: {context.Response.StatusCode}",
            });
            await db.SaveChangesAsync();
        }
        catch { /* audit failure should not break the request */ }
    }
}
