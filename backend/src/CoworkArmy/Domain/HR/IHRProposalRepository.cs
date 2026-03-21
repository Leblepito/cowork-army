namespace CoworkArmy.Domain.HR;

public interface IHRProposalRepository
{
    Task<HRProposal?> GetByIdAsync(string id);
    Task<List<HRProposal>> GetPendingAsync();
    Task AddAsync(HRProposal proposal);
    Task UpdateAsync(HRProposal proposal);
    Task<bool> ExistsPendingAsync(string agentId, ProposalType type);
}
