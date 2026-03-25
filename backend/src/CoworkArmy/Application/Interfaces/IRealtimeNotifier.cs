using CoworkArmy.Domain.Agents;

namespace CoworkArmy.Application.Interfaces;

public interface IRealtimeNotifier
{
    Task SendEventAsync(string type, string agentId, string message);
    Task SendStatusChangeAsync(string agentId, string status);
    Task SendConversationAsync(string fromId, string fromIcon, string toId, string message);
    Task SendCommandAsync(string phase, string fromId, string toId, string message);
    Task SendBudgetWarningAsync(string level, decimal current, decimal cap);
    Task SendAgentSpawnedAsync(string agentId, string name, string icon, string department, string spawnedBy);
    Task SendAgentRetiredAsync(string agentId, string reason);
    Task SendAgentMessageAsync(string fromId, string toId, string content);
    Task SendMovementAsync(string agentId, string targetAgentId, int durationMs);
    Task SendDocumentTransferAsync(string fromId, string toId, string docType);
    Task SendTaskEffectAsync(string agentId, string effect);
    Task SendTradeFeedAsync(CoworkArmy.Domain.DataBridge.TradeFeed feed);
    Task SendMedicalFeedAsync(CoworkArmy.Domain.DataBridge.MedicalFeed feed);
    Task SendHotelFeedAsync(CoworkArmy.Domain.DataBridge.HotelFeed feed);
    Task SendChatTypingAsync(string agentId, bool isTyping);
    Task SendChatMessageAsync(string agentId, string agentIcon, string role, string content);
    Task SendClaudeActionAsync(string tool, string? file, string agentId, string summary);
    Task SendClaudeTaskStartAsync(string taskId, string title, string scope, string[] agents);
    Task SendClaudeTaskCompleteAsync(string taskId, string status, int durationMs);
}

public interface IStatusTracker
{
    AgentStatus Get(string id);
    Dictionary<string, AgentStatus> GetAll();
    void Set(string id, string status, string? line = null);
    void AddLog(string id, string line);
    void Init(IEnumerable<string> ids);
}

public interface ITaskRouter
{
    string Route(string text);
}

public interface IAutonomousEngine
{
    bool Running { get; }
    int TickCount { get; }
    void Start();
    void Stop();
}
