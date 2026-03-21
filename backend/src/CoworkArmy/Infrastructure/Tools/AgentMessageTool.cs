using CoworkArmy.Domain.Tools;
using CoworkArmy.Domain.Messages;
namespace CoworkArmy.Infrastructure.Tools;

public class AgentMessageTool : ITool
{
    private readonly IMessageBus _bus;
    public string Name => "agent_message";
    public string Description => "Send message to another agent. Params: to_agent, message";
    public ToolPermission Permission => ToolPermission.Safe;
    public string[] RequiredParams => new[] { "to_agent", "message" };

    public AgentMessageTool(IMessageBus bus) => _bus = bus;

    public async Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("to_agent", out var toId) || !parameters.TryGetValue("message", out var message))
            return new ToolResult(false, "", Error: "Missing 'to_agent' or 'message'");
        var fromId = parameters.GetValueOrDefault("_from_agent", "system");
        var msg = new AgentMessage { FromId = fromId, ToId = toId, Type = MessageType.Request, Content = message };
        var sent = await _bus.SendAsync(msg);
        return sent ? new ToolResult(true, $"Message sent to {toId}") : new ToolResult(false, "", Error: "Message rejected");
    }
}
