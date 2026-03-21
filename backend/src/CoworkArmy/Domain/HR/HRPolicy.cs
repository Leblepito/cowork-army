namespace CoworkArmy.Domain.HR;

public enum HRActionType { Auto, Proposal }
public enum ProposalType { Spawn, Retire, Review, Warning }
public enum ProposalStatus { Pending, Approved, Rejected }

public class HRProposal
{
    public string Id { get; set; } = $"hr-{Guid.NewGuid().ToString("N")[..6]}";
    public ProposalType Type { get; set; }
    public string? AgentId { get; set; }
    public string Reason { get; set; } = "";
    public string Details { get; set; } = "{}";
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public void Approve() { Status = ProposalStatus.Approved; ResolvedAt = DateTime.UtcNow; }
    public void Reject() { Status = ProposalStatus.Rejected; ResolvedAt = DateTime.UtcNow; }
}

public static class HRRules
{
    public const int ConsecutiveFailuresForWarning = 5;
    public const int WarningsForRetire = 3;
    public const int IdleDaysForReview = 7;
    public const int IdleDaysForDuplicateRetire = 3;
    public const double UtilizationThresholdForNewWorker = 0.8;
    public const double CostMultiplierForWarning = 3.0;
}
