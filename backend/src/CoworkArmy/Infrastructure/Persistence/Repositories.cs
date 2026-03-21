using Microsoft.EntityFrameworkCore;
using CoworkArmy.Domain;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Tasks;
using CoworkArmy.Domain.Events;
using CoworkArmy.Domain.HR;

namespace CoworkArmy.Infrastructure.Persistence;

// ═══ Agent Repository ═══
public class AgentRepository : IAgentRepository
{
    private readonly CoworkDbContext _db;
    public AgentRepository(CoworkDbContext db) => _db = db;

    public async Task<Agent?> GetByIdAsync(string id)
        => await _db.Agents.FindAsync(id);

    public async Task<List<Agent>> GetAllAsync()
        => await _db.Agents.OrderBy(a => a.Department).ThenByDescending(a => a.Tier).ToListAsync();

    public async Task<List<Agent>> GetByDepartmentAsync(string dept)
        => await _db.Agents.Where(a => a.Department == dept).ToListAsync();

    public async Task AddAsync(Agent agent)
    {
        _db.Agents.Add(agent);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var agent = await _db.Agents.FindAsync(id);
        if (agent != null) { _db.Agents.Remove(agent); await _db.SaveChangesAsync(); }
    }

    public async Task UpdateAsync(Agent agent)
    {
        _db.Agents.Update(agent);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string id)
        => await _db.Agents.AnyAsync(a => a.Id == id);
}

// ═══ Task Repository ═══
public class TaskRepository : ITaskRepository
{
    private readonly CoworkDbContext _db;
    public TaskRepository(CoworkDbContext db) => _db = db;

    public async Task<AgentTask?> GetByIdAsync(string id)
        => await _db.Tasks.FindAsync(id);

    public async Task<List<AgentTask>> GetRecentAsync(int limit = 50, int offset = 0)
    {
        var query = _db.Tasks.OrderByDescending(t => t.CreatedAt);
        if (offset > 0) return await ((IQueryable<AgentTask>)query).Skip(offset).Take(limit).ToListAsync();
        return await query.Take(limit).ToListAsync();
    }

    public async Task AddAsync(AgentTask task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AgentTask task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
    }
}

// ═══ Event Repository ═══
public class EventRepository : IEventRepository
{
    private readonly CoworkDbContext _db;
    public EventRepository(CoworkDbContext db) => _db = db;

    public async Task AddAsync(AgentEvent evt)
    {
        _db.Events.Add(evt);
        await _db.SaveChangesAsync();
    }

    public async Task<List<AgentEvent>> GetRecentAsync(int limit = 50, int offset = 0)
    {
        var query = _db.Events.OrderByDescending(e => e.Timestamp);
        if (offset > 0) return await ((IQueryable<AgentEvent>)query).Skip(offset).Take(limit).ToListAsync();
        return await query.Take(limit).ToListAsync();
    }
}

// ═══ HR Proposal Repository ═══
public class HRProposalRepository : IHRProposalRepository
{
    private readonly CoworkDbContext _db;
    public HRProposalRepository(CoworkDbContext db) => _db = db;

    public async Task<HRProposal?> GetByIdAsync(string id)
        => await _db.HRProposals.FindAsync(id);

    public async Task<List<HRProposal>> GetPendingAsync()
        => await _db.HRProposals.Where(p => p.Status == ProposalStatus.Pending).ToListAsync();

    public async Task AddAsync(HRProposal proposal)
    {
        _db.HRProposals.Add(proposal);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(HRProposal proposal)
    {
        _db.HRProposals.Update(proposal);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsPendingAsync(string agentId, ProposalType type)
        => await _db.HRProposals.AnyAsync(p => p.AgentId == agentId && p.Type == type && p.Status == ProposalStatus.Pending);
}

// ═══ Agent State Repository ═══
public class AgentStateRepository : IAgentStateRepository
{
    private readonly CoworkDbContext _db;
    public AgentStateRepository(CoworkDbContext db) => _db = db;

    public async Task<AgentState?> GetByAgentIdAsync(string agentId)
        => await _db.AgentStates.FindAsync(agentId);

    public async Task AddAsync(AgentState state)
    {
        _db.AgentStates.Add(state);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AgentState state)
    {
        _db.AgentStates.Update(state);
        await _db.SaveChangesAsync();
    }
}

// ═══ Performance Repository ═══
public class PerformanceRepository : IPerformanceRepository
{
    private readonly CoworkDbContext _db;
    public PerformanceRepository(CoworkDbContext db) => _db = db;

    public async Task<AgentPerformance?> GetByAgentIdAsync(string agentId)
        => await _db.AgentPerformance.FindAsync(agentId);

    public async Task<List<AgentPerformance>> GetAllAsync()
        => await _db.AgentPerformance.ToListAsync();

    public async Task AddAsync(AgentPerformance perf)
    {
        _db.AgentPerformance.Add(perf);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AgentPerformance perf)
    {
        _db.AgentPerformance.Update(perf);
        await _db.SaveChangesAsync();
    }
}
