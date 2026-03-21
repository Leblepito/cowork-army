using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Messages;

namespace CoworkArmy.Infrastructure.Messaging;

public class ChannelMessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<string, Channel<AgentMessage>> _channels = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<AgentMessage>> _waiters = new();
    private readonly IServiceProvider _sp;
    private readonly ILogger<ChannelMessageBus> _log;

    private static readonly Dictionary<AgentTier, int> TierRank = new()
    {
        [AgentTier.CEO] = 3, [AgentTier.DIR] = 2, [AgentTier.WRK] = 1, [AgentTier.SUP] = 1,
    };

    public ChannelMessageBus(IServiceProvider sp, ILogger<ChannelMessageBus> log) { _sp = sp; _log = log; }

    public void EnsureChannel(string agentId)
    {
        _channels.GetOrAdd(agentId, _ => Channel.CreateUnbounded<AgentMessage>());
    }

    public async Task<bool> SendAsync(AgentMessage message)
    {
        if (!await ValidateHierarchy(message))
        {
            _log.LogWarning("Message rejected: {From}→{To} type={Type}", message.FromId, message.ToId, message.Type);
            return false;
        }
        EnsureChannel(message.ToId);
        await _channels[message.ToId].Writer.WriteAsync(message);

        using var scope = _sp.CreateScope();
        var notifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
        await notifier.SendEventAsync("message", message.FromId,
            $"{message.FromId}→{message.ToId}: {message.Content[..Math.Min(50, message.Content.Length)]}");
        await notifier.SendAgentMessageAsync(message.FromId, message.ToId,
            message.Content[..Math.Min(200, message.Content.Length)]);

        var db = scope.ServiceProvider.GetRequiredService<CoworkArmy.Infrastructure.Persistence.CoworkDbContext>();
        db.AgentMessages.Add(new AgentMessageEntity
        {
            FromId = message.FromId, ToId = message.ToId,
            Type = message.Type.ToString().ToLower(), Content = message.Content,
            Priority = message.Priority.ToString().ToLower(), Timestamp = message.Timestamp,
        });
        await db.SaveChangesAsync();

        if (_waiters.TryGetValue(message.Id, out var tcs))
        { tcs.TrySetResult(message); _waiters.TryRemove(message.Id, out _); }

        _log.LogInformation("Message: {From}→{To} type={Type}", message.FromId, message.ToId, message.Type);
        return true;
    }

    public async IAsyncEnumerable<AgentMessage> SubscribeAsync(string agentId,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        EnsureChannel(agentId);
        await foreach (var msg in _channels[agentId].Reader.ReadAllAsync(ct))
            yield return msg;
    }

    public async Task<AgentMessage?> WaitForResponseAsync(string messageId, TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<AgentMessage>();
        _waiters[messageId] = tcs;
        using var cts = new CancellationTokenSource(timeout);
        cts.Token.Register(() => tcs.TrySetCanceled());
        try { return await tcs.Task; }
        catch (TaskCanceledException) { _waiters.TryRemove(messageId, out _); return null; }
    }

    private async Task<bool> ValidateHierarchy(AgentMessage message)
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IAgentRepository>();
        var from = await repo.GetByIdAsync(message.FromId);
        var to = await repo.GetByIdAsync(message.ToId);
        if (from == null || to == null) return false;
        var fromRank = TierRank.GetValueOrDefault(from.Tier, 0);
        var toRank = TierRank.GetValueOrDefault(to.Tier, 0);
        if (from.Id == "cargo" && message.Type == MessageType.Info) return true;
        return message.Type switch
        {
            MessageType.Command => fromRank > toRank,
            MessageType.Response => fromRank < toRank,
            MessageType.Info => fromRank < toRank,
            MessageType.Request => fromRank <= toRank || from.Department == to.Department,
            _ => false,
        };
    }
}

public class AgentMessageEntity
{
    public int Id { get; set; }
    public string FromId { get; set; } = "";
    public string ToId { get; set; } = "";
    public string Type { get; set; } = "info";
    public string Content { get; set; } = "";
    public string Priority { get; set; } = "normal";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
