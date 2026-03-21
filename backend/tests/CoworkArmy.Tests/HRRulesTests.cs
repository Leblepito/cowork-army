using CoworkArmy.Domain.HR;
using Xunit;

namespace CoworkArmy.Tests;

public class HRRulesTests
{
    [Fact]
    public void Constants_have_expected_values()
    {
        Assert.Equal(5, HRRules.ConsecutiveFailuresForWarning);
        Assert.Equal(3, HRRules.WarningsForRetire);
        Assert.Equal(7, HRRules.IdleDaysForReview);
        Assert.Equal(3, HRRules.IdleDaysForDuplicateRetire);
        Assert.Equal(3.0, HRRules.CostMultiplierForWarning);
    }

    [Fact]
    public void Proposal_approve_sets_status_and_timestamp()
    {
        var p = new HRProposal { Reason = "test" };
        Assert.Equal(ProposalStatus.Pending, p.Status);
        p.Approve();
        Assert.Equal(ProposalStatus.Approved, p.Status);
        Assert.NotNull(p.ResolvedAt);
    }

    [Fact]
    public void Proposal_reject_sets_status()
    {
        var p = new HRProposal { Reason = "test" };
        p.Reject();
        Assert.Equal(ProposalStatus.Rejected, p.Status);
    }

    [Fact]
    public void Proposal_id_has_hr_prefix()
    {
        var p = new HRProposal { Reason = "test" };
        Assert.StartsWith("hr-", p.Id);
    }
}
