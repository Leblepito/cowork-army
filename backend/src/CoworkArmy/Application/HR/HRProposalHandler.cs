using CoworkArmy.Domain.HR;

namespace CoworkArmy.Application.HR;

public class HRProposalHandler
{
    private readonly IHRProposalRepository _proposals;
    private readonly HRSpawnHandler _spawner;
    private readonly HRRetireHandler _retirer;

    public HRProposalHandler(IHRProposalRepository proposals, HRSpawnHandler spawner, HRRetireHandler retirer)
    { _proposals = proposals; _spawner = spawner; _retirer = retirer; }

    public Task<List<HRProposal>> GetPendingAsync()
        => _proposals.GetPendingAsync();

    public async Task ApproveAsync(string proposalId)
    {
        var proposal = await _proposals.GetByIdAsync(proposalId)
            ?? throw new Exception($"Proposal not found: {proposalId}");

        proposal.Approve();

        switch (proposal.Type)
        {
            case ProposalType.Retire when proposal.AgentId != null:
                await _retirer.HandleAsync(proposal.AgentId, proposal.Reason);
                break;
            case ProposalType.Spawn:
                await _spawner.HandleAsync(proposal.Reason, "general");
                break;
        }

        await _proposals.UpdateAsync(proposal);
    }

    public async Task RejectAsync(string proposalId)
    {
        var proposal = await _proposals.GetByIdAsync(proposalId)
            ?? throw new Exception($"Proposal not found: {proposalId}");
        proposal.Reject();
        await _proposals.UpdateAsync(proposal);
    }
}
