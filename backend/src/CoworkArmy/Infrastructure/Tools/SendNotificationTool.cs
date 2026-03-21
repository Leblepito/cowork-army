using CoworkArmy.Domain.Tools;

namespace CoworkArmy.Infrastructure.Tools;

public class SendNotificationTool : ITool
{
    private readonly ILogger<SendNotificationTool> _log;

    public string Name => "send_notification";
    public string Description => "Send a notification to a user or channel. Params: channel, message, priority (optional: low|normal|high)";
    public ToolPermission Permission => ToolPermission.Elevated;
    public string[] RequiredParams => new[] { "channel", "message" };

    public SendNotificationTool(ILogger<SendNotificationTool> log) => _log = log;

    public Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("channel", out var channel) || !parameters.TryGetValue("message", out var message))
            return Task.FromResult(new ToolResult(false, "", Error: "Missing 'channel' or 'message'"));

        var priority = parameters.GetValueOrDefault("priority", "normal");
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

        _log.LogInformation("Notification sent → channel={Channel}, priority={Priority}, message={Message}",
            channel, priority, message);

        var output = $"[{timestamp}] Notification dispatched to '{channel}' (priority={priority}): {message}";
        return Task.FromResult(new ToolResult(true, output, TokensUsed: 0));
    }
}
