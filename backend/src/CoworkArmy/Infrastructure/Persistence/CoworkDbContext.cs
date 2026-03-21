using Microsoft.EntityFrameworkCore;
using CoworkArmy.Domain;
using CoworkArmy.Domain.Agents;
using CoworkArmy.Domain.Tasks;
using CoworkArmy.Domain.Events;
using CoworkArmy.Infrastructure.Messaging;
using CoworkArmy.Domain.Chat;

namespace CoworkArmy.Infrastructure.Persistence;

public class CoworkDbContext : DbContext
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<AgentTask> Tasks => Set<AgentTask>();
    public DbSet<AgentEvent> Events => Set<AgentEvent>();
    public DbSet<LlmUsageEntry> LlmUsage => Set<LlmUsageEntry>();
    public DbSet<AgentState> AgentStates => Set<AgentState>();
    public DbSet<AgentMessageEntity> AgentMessages => Set<AgentMessageEntity>();
    public DbSet<AgentPerformance> AgentPerformance => Set<AgentPerformance>();
    public DbSet<Domain.HR.HRProposal> HRProposals => Set<Domain.HR.HRProposal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ChatConversation> ChatConversations => Set<ChatConversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    public CoworkDbContext(DbContextOptions<CoworkDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Agent>(e =>
        {
            e.ToTable("agents");
            e.HasKey(a => a.Id);
            e.Property(a => a.Tier).HasConversion<string>();
            e.HasIndex(a => a.Department);
            e.HasIndex(a => a.Tier);
            e.Property(a => a.IsActive).HasDefaultValue(true);
            e.Property(a => a.IsImmortal).HasDefaultValue(false);
            e.Property(a => a.Tools).HasDefaultValue("[]");
            e.Property(a => a.CreatedBy).HasDefaultValue("system");
        });

        b.Entity<AgentTask>(e =>
        {
            e.ToTable("agent_tasks");
            e.HasKey(t => t.Id);
            e.Property(t => t.Priority).HasConversion<string>();
            e.Property(t => t.Status).HasConversion<string>();
            e.HasIndex(t => t.Status);
            e.HasIndex(t => t.AssignedTo);
            e.HasIndex(t => t.CreatedAt);
        });

        b.Entity<AgentEvent>(e =>
        {
            e.ToTable("agent_events");
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Id).ValueGeneratedOnAdd();
            e.HasIndex(ev => ev.Timestamp);
            e.HasIndex(ev => ev.AgentId);
        });

        b.Entity<LlmUsageEntry>(e =>
        {
            e.ToTable("llm_usage");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedOnAdd();
            e.HasIndex(u => u.AgentId);
            e.HasIndex(u => u.Timestamp);
        });

        b.Entity<AgentState>(e =>
        {
            e.ToTable("agent_states");
            e.HasKey(s => s.AgentId);
        });

        b.Entity<AgentMessageEntity>(e =>
        {
            e.ToTable("agent_messages");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).ValueGeneratedOnAdd();
            e.HasIndex(m => m.FromId);
            e.HasIndex(m => m.ToId);
            e.HasIndex(m => m.Timestamp);
        });

        b.Entity<AgentPerformance>(e =>
        {
            e.ToTable("agent_performance");
            e.HasKey(p => p.AgentId);
        });

        b.Entity<Domain.HR.HRProposal>(e =>
        {
            e.ToTable("hr_proposals");
            e.HasKey(p => p.Id);
            e.Property(p => p.Type).HasConversion<string>();
            e.Property(p => p.Status).HasConversion<string>();
        });

        b.Entity<ChatConversation>(e =>
        {
            e.ToTable("chat_conversations");
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.AgentId);
            e.HasIndex(c => c.UpdatedAt);
            e.HasMany(c => c.Messages)
                .WithOne()
                .HasForeignKey(m => m.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ChatMessage>(e =>
        {
            e.ToTable("chat_messages");
            e.HasKey(m => m.Id);
            e.HasIndex(m => m.ConversationId);
            e.HasIndex(m => m.Timestamp);
        });
    }
}
