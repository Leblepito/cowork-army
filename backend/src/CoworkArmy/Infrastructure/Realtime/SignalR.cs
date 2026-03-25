using Microsoft.AspNetCore.SignalR;
using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.Realtime;

public class CoworkHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", new { message = "COWORK.ARMY bağlandı", time = DateTime.UtcNow });
        await base.OnConnectedAsync();
    }
}

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<CoworkHub> _hub;
    public SignalRNotifier(IHubContext<CoworkHub> hub) => _hub = hub;

    public Task SendEventAsync(string type, string agentId, string message)
        => _hub.Clients.All.SendAsync("AgentEvent", new { type, agentId, message, timestamp = DateTime.UtcNow });

    public Task SendStatusChangeAsync(string agentId, string status)
        => _hub.Clients.All.SendAsync("StatusChange", new { agentId, status, timestamp = DateTime.UtcNow });

    public Task SendConversationAsync(string fromId, string fromIcon, string toId, string message)
        => _hub.Clients.All.SendAsync("Conversation", new { fromId, fromIcon, toId, message, timestamp = DateTime.UtcNow });

    public Task SendCommandAsync(string phase, string fromId, string toId, string message)
        => _hub.Clients.All.SendAsync("Command", new { phase, fromId, toId, message, timestamp = DateTime.UtcNow });

    public Task SendBudgetWarningAsync(string level, decimal current, decimal cap)
        => _hub.Clients.All.SendAsync("BudgetWarning", new { level, current, cap });

    public Task SendAgentSpawnedAsync(string agentId, string name, string icon, string department, string spawnedBy)
        => _hub.Clients.All.SendAsync("AgentSpawned", new { agentId, name, icon, department, spawnedBy });

    public Task SendAgentRetiredAsync(string agentId, string reason)
        => _hub.Clients.All.SendAsync("AgentRetired", new { agentId, reason });

    public Task SendAgentMessageAsync(string fromId, string toId, string content)
        => _hub.Clients.All.SendAsync("AgentMessage", new { fromId, toId, content });

    public Task SendMovementAsync(string agentId, string targetAgentId, int durationMs)
        => _hub.Clients.All.SendAsync("AgentMovement", new { agentId, targetAgentId, duration = durationMs });

    public Task SendDocumentTransferAsync(string fromId, string toId, string docType)
        => _hub.Clients.All.SendAsync("DocumentTransfer", new { fromId, toId, docType });

    public Task SendTaskEffectAsync(string agentId, string effect)
        => _hub.Clients.All.SendAsync("TaskEffect", new { agentId, effect });

    public Task SendTradeFeedAsync(CoworkArmy.Domain.DataBridge.TradeFeed feed)
        => _hub.Clients.All.SendAsync("TradeFeed", new
        {
            btcPrice = feed.BtcPrice, ethPrice = feed.EthPrice,
            btcChange24h = feed.BtcChange24h, ethChange24h = feed.EthChange24h,
            openPositions = feed.OpenPositions, totalPnl = feed.TotalPnl,
            activeSignals = feed.ActiveSignals, fetchedAt = feed.FetchedAt
        });

    public Task SendMedicalFeedAsync(CoworkArmy.Domain.DataBridge.MedicalFeed feed)
        => _hub.Clients.All.SendAsync("MedicalFeed", new
        {
            patientsToday = feed.PatientsToday, surgeryQueue = feed.SurgeryQueue,
            vipPipeline = feed.VipPipeline, monthlyRevenue = feed.MonthlyRevenue,
            partnerHospitals = feed.PartnerHospitals, countriesServed = feed.CountriesServed,
            fetchedAt = feed.FetchedAt
        });

    public Task SendHotelFeedAsync(CoworkArmy.Domain.DataBridge.HotelFeed feed)
        => _hub.Clients.All.SendAsync("HotelFeed", new
        {
            occupancyPercent = feed.OccupancyPercent, totalRooms = feed.TotalRooms,
            checkInsToday = feed.CheckInsToday, checkOutsToday = feed.CheckOutsToday,
            newReservations = feed.NewReservations, revPar = feed.RevPar,
            tours = feed.Tours, transfers = feed.Transfers,
            spaBookings = feed.SpaBookings, restaurantReservations = feed.RestaurantReservations,
            fetchedAt = feed.FetchedAt
        });

    public Task SendChatTypingAsync(string agentId, bool isTyping)
        => _hub.Clients.All.SendAsync("ChatTyping", new { agentId, isTyping, timestamp = DateTime.UtcNow });

    public Task SendChatMessageAsync(string agentId, string agentIcon, string role, string content)
        => _hub.Clients.All.SendAsync("ChatMessage", new { agentId, agentIcon, role, content, timestamp = DateTime.UtcNow });

    public async Task SendClaudeActionAsync(string tool, string? file, string agentId, string summary)
        => await _hub.Clients.All.SendAsync("ClaudeAction", new { tool, file, agentId, summary, timestamp = DateTimeOffset.UtcNow });

    public async Task SendClaudeTaskStartAsync(string taskId, string title, string scope, string[] agents)
        => await _hub.Clients.All.SendAsync("ClaudeTaskStart", new { taskId, title, scope, agents, timestamp = DateTimeOffset.UtcNow });

    public async Task SendClaudeTaskCompleteAsync(string taskId, string status, int durationMs)
        => await _hub.Clients.All.SendAsync("ClaudeTaskComplete", new { taskId, status, durationMs, timestamp = DateTimeOffset.UtcNow });
}
