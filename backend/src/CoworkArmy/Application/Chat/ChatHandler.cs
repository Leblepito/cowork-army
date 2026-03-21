using CoworkArmy.Application.Chat.DTOs;
using CoworkArmy.Application.Interfaces;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Chat;
using CoworkArmy.Domain.DataBridge;
using CoworkArmy.Domain.Events;

namespace CoworkArmy.Application.Chat;

public record ChatCommand(string AgentId, string Message);
public record ChatResponse(string Response, int Tokens, double Cost);

public class ChatHandler
{
    private readonly IAgentRepository _agents;
    private readonly ILlmProviderFactory _llmFactory;
    private readonly IBudgetGuard _budget;
    private readonly IRealtimeNotifier _notifier;
    private readonly IEventRepository _events;
    private readonly IDataBridgeService _bridge;
    private readonly IChatRepository _chatRepo;
    private readonly IStatusTracker _tracker;

    public ChatHandler(
        IAgentRepository agents, ILlmProviderFactory llmFactory, IBudgetGuard budget,
        IRealtimeNotifier notifier, IEventRepository events,
        IDataBridgeService bridge, IChatRepository chatRepo, IStatusTracker tracker)
    {
        _agents = agents; _llmFactory = llmFactory; _budget = budget;
        _notifier = notifier; _events = events;
        _bridge = bridge; _chatRepo = chatRepo; _tracker = tracker;
    }

    /// <summary>Original simple chat (backward compatible).</summary>
    public async Task<ChatResponse> HandleAsync(ChatCommand cmd)
    {
        var agent = await _agents.GetByIdAsync(cmd.AgentId)
            ?? throw new CoworkArmy.Domain.Common.DomainException($"Agent not found: {cmd.AgentId}");

        if (!await _budget.CanSpendAsync(cmd.AgentId))
            throw new CoworkArmy.Domain.Common.DomainException("Budget cap exceeded for this agent");

        var llm = _llmFactory.GetByTier(agent.Tier.ToString());
        var model = SelectModel(llm.Name, agent.Tier);

        // Build enriched system prompt with Data Bridge data
        var systemPrompt = await BuildSystemPromptAsync(agent);

        var request = new LlmRequest(
            Model: model,
            SystemPrompt: systemPrompt,
            Messages: new List<LlmMessage> { new("user", cmd.Message) },
            MaxTokens: 1024);

        _tracker.Set(agent.Id, "thinking", $"💭 {cmd.Message[..Math.Min(30, cmd.Message.Length)]}...");
        await _notifier.SendStatusChangeAsync(cmd.AgentId, "thinking");
        await _notifier.SendChatTypingAsync(cmd.AgentId, true);
        await _notifier.SendEventAsync("work", cmd.AgentId, $"{agent.Icon} düşünüyor...");

        LlmResponse response;
        try
        {
            response = await llm.SendAsync(request);
        }
        catch (Exception ex)
        {
            await _notifier.SendChatTypingAsync(cmd.AgentId, false);
            await _notifier.SendStatusChangeAsync(cmd.AgentId, "idle");
            throw new CoworkArmy.Domain.Common.DomainException($"LLM error: {ex.Message}");
        }

        await _budget.RecordUsageAsync(cmd.AgentId, llm.Name, model,
            response.InputTokens, response.OutputTokens, response.CostUsd);

        await _events.AddAsync(new AgentEvent
        {
            Type = "response",
            AgentId = cmd.AgentId,
            Message = $"{agent.Icon}: {response.Content[..Math.Min(80, response.Content.Length)]}"
        });

        _tracker.Set(agent.Id, "idle", $"💬 {response.Content[..Math.Min(50, response.Content.Length)]}...");
        await _notifier.SendChatTypingAsync(cmd.AgentId, false);
        await _notifier.SendChatMessageAsync(cmd.AgentId, agent.Icon, "assistant", response.Content);
        await _notifier.SendConversationAsync(cmd.AgentId, agent.Icon, "user", response.Content);
        await _notifier.SendStatusChangeAsync(cmd.AgentId, "idle");

        return new ChatResponse(response.Content, response.InputTokens + response.OutputTokens, response.CostUsd);
    }

    /// <summary>Chat with conversation persistence.</summary>
    public async Task<ChatResponseDto> HandleWithConversationAsync(SendMessageDto dto)
    {
        var agent = await _agents.GetByIdAsync(dto.AgentId)
            ?? throw new CoworkArmy.Domain.Common.DomainException($"Agent not found: {dto.AgentId}");

        if (!await _budget.CanSpendAsync(dto.AgentId))
            throw new CoworkArmy.Domain.Common.DomainException("Budget cap exceeded for this agent");

        // Get or create conversation
        ChatConversation conv;
        if (!string.IsNullOrEmpty(dto.ConversationId))
        {
            conv = await _chatRepo.GetByIdAsync(dto.ConversationId)
                ?? ChatConversation.Create(dto.AgentId);
        }
        else
        {
            conv = ChatConversation.Create(dto.AgentId,
                dto.Message.Length > 40 ? dto.Message[..40] + "..." : dto.Message);
        }

        conv.AddMessage("user", dto.Message);

        var llm = _llmFactory.GetByTier(agent.Tier.ToString());
        var model = SelectModel(llm.Name, agent.Tier);
        var systemPrompt = await BuildSystemPromptAsync(agent);

        // Build message history
        var messages = conv.Messages
            .Where(m => m.Role != "system")
            .Select(m => new LlmMessage(m.Role, m.Content))
            .ToList();

        _tracker.Set(agent.Id, "thinking", $"💭 {dto.Message[..Math.Min(30, dto.Message.Length)]}...");
        await _notifier.SendStatusChangeAsync(dto.AgentId, "thinking");
        await _notifier.SendChatTypingAsync(dto.AgentId, true);

        LlmResponse response;
        try
        {
            var request = new LlmRequest(Model: model, SystemPrompt: systemPrompt,
                Messages: messages, MaxTokens: 1024);
            response = await llm.SendAsync(request);
        }
        catch (Exception ex)
        {
            conv.AddMessage("assistant", $"[Hata: {ex.Message}] Şu an yanıt veremiyorum.");
            if (string.IsNullOrEmpty(dto.ConversationId))
                await _chatRepo.AddAsync(conv);
            else
                await _chatRepo.UpdateAsync(conv);

            await _notifier.SendChatTypingAsync(dto.AgentId, false);
            await _notifier.SendStatusChangeAsync(dto.AgentId, "idle");

            var errUser = conv.Messages[^2];
            var errAsst = conv.Messages[^1];
            return new ChatResponseDto(conv.Id,
                new ChatMessageResponseDto(errUser.Id, errUser.Role, errUser.Content, errUser.Timestamp),
                new ChatMessageResponseDto(errAsst.Id, errAsst.Role, errAsst.Content, errAsst.Timestamp),
                0, 0);
        }

        conv.AddMessage("assistant", response.Content);

        // Save
        if (string.IsNullOrEmpty(dto.ConversationId))
            await _chatRepo.AddAsync(conv);
        else
            await _chatRepo.UpdateAsync(conv);

        await _budget.RecordUsageAsync(dto.AgentId, llm.Name, model,
            response.InputTokens, response.OutputTokens, response.CostUsd);

        await _events.AddAsync(new AgentEvent
        {
            Type = "response",
            AgentId = dto.AgentId,
            Message = $"{agent.Icon}: {response.Content[..Math.Min(80, response.Content.Length)]}"
        });

        _tracker.Set(agent.Id, "idle", $"💬 {response.Content[..Math.Min(50, response.Content.Length)]}...");
        await _notifier.SendChatTypingAsync(dto.AgentId, false);
        await _notifier.SendChatMessageAsync(dto.AgentId, agent.Icon, "assistant", response.Content);
        await _notifier.SendStatusChangeAsync(dto.AgentId, "idle");

        var userMsg = conv.Messages[^2];
        var assistantMsg = conv.Messages[^1];

        return new ChatResponseDto(conv.Id,
            new ChatMessageResponseDto(userMsg.Id, userMsg.Role, userMsg.Content, userMsg.Timestamp),
            new ChatMessageResponseDto(assistantMsg.Id, assistantMsg.Role, assistantMsg.Content, assistantMsg.Timestamp),
            response.InputTokens + response.OutputTokens, response.CostUsd);
    }

    private static string SelectModel(string provider, AgentTier tier) => provider switch
    {
        "anthropic" => tier >= AgentTier.DIR ? "claude-sonnet-4-6" : "claude-haiku-4-5",
        "gemini" => tier >= AgentTier.DIR ? "gemini-2.5-flash" : "gemini-2.5-flash",
        "openai" => tier >= AgentTier.DIR ? "gpt-4o" : "gpt-4o-mini",
        _ => "gemini-2.5-flash"
    };

    private async Task<string> BuildSystemPromptAsync(Domain.Agents.Agent agent)
    {
        var basePrompt = agent.SystemPrompt;
        var dept = agent.Department;
        var dataContext = "";

        try
        {
            if (dept is "trade" or "hq")
            {
                var trade = await _bridge.GetTradeFeedAsync();
                dataContext += $@"

GERÇEK ZAMANLI VERİ (Data Bridge — u2algo.com):
- BTC Fiyat: ${trade.BtcPrice:N2} (24h değişim: {trade.BtcChange24h:+0.0;-0.0}%)
- ETH Fiyat: ${trade.EthPrice:N2} (24h değişim: {trade.EthChange24h:+0.0;-0.0}%)
- Açık Pozisyon: {trade.OpenPositions} adet
- Toplam P&L: ${trade.TotalPnl:N2}
- Aktif Sinyal: {trade.ActiveSignals} adet
- Veri zamanı: {trade.FetchedAt:HH:mm:ss UTC}";
            }

            if (dept is "medical" or "hq")
            {
                var med = await _bridge.GetMedicalFeedAsync();
                dataContext += $@"

GERÇEK ZAMANLI VERİ (Data Bridge — leblepito.com/medical):
- Bugünkü Hastalar: {med.PatientsToday}
- Ameliyat Kuyruğu: {med.SurgeryQueue}
- VIP Pipeline: {med.VipPipeline}
- Aylık Gelir: ${med.MonthlyRevenue:N0}
- Partner Hastaneler: {med.PartnerHospitals}
- Ülke: {med.CountriesServed}
- Veri zamanı: {med.FetchedAt:HH:mm:ss UTC}";
            }

            if (dept is "hotel" or "hq")
            {
                var hotel = await _bridge.GetHotelFeedAsync();
                dataContext += $@"

GERÇEK ZAMANLI VERİ (Data Bridge — leblepito.com/hotel):
- Oda Doluluk: %{hotel.OccupancyPercent} ({hotel.TotalRooms} toplam oda)
- Bugün Check-in: {hotel.CheckInsToday} | Check-out: {hotel.CheckOutsToday}
- Yeni Rezervasyon: {hotel.NewReservations}
- RevPAR: ${hotel.RevPar:N0}
- Turlar: {hotel.Tours} | Transferler: {hotel.Transfers}
- Veri zamanı: {hotel.FetchedAt:HH:mm:ss UTC}";
            }
        }
        catch
        {
            dataContext = "\n[Data Bridge bağlantı hatası — simülasyon modunda]";
        }

        return $@"{basePrompt}
{dataContext}

KURALLAR:
- Türkçe yanıt ver.
- Gerçek verilere dayanarak analiz yap.
- Kısa ve öz cevaplar ver (max 3 paragraf).
- Somut öneriler sun, belirsiz konuşma.
- Rakamlarla destekle.";
    }
}
