using System.Collections.Concurrent;

namespace CoworkArmy.API.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, Queue<DateTime>> _requests = new();
    private const int MaxRequests = 60;
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

    public RateLimitMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Method != "POST")
        {
            await _next(context);
            return;
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = $"{ip}:{context.Request.Path}";
        var now = DateTime.UtcNow;

        var queue = _requests.GetOrAdd(key, _ => new Queue<DateTime>());

        lock (queue)
        {
            while (queue.Count > 0 && now - queue.Peek() > Window)
                queue.Dequeue();

            if (queue.Count >= MaxRequests)
            {
                context.Response.StatusCode = 429;
                context.Response.ContentType = "application/json";
                context.Response.WriteAsync("{\"error\":\"Rate limit exceeded\"}");
                return;
            }

            queue.Enqueue(now);
        }

        await _next(context);
    }
}
