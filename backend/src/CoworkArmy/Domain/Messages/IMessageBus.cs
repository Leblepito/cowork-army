namespace CoworkArmy.Domain.Messages;

public interface IMessageBus
{
    Task<bool> SendAsync(AgentMessage message);
    IAsyncEnumerable<AgentMessage> SubscribeAsync(string agentId, CancellationToken ct = default);
    Task<AgentMessage?> WaitForResponseAsync(string messageId, TimeSpan timeout);
}
