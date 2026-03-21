namespace CoworkArmy.Domain.HR;

public interface IHRPolicyEngine
{
    Task<List<HRProposal>> EvaluateAsync();
    Task ExecuteAutoActionsAsync();
    Task<HRProposal> SpawnAgentAsync(string reason, string department);
    Task RetireAgentAsync(string agentId, string reason);
    Task WarnAgentAsync(string agentId, string reason);
}
