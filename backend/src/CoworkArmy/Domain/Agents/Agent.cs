using CoworkArmy.Domain.Common;
namespace CoworkArmy.Domain.Agents;
public class Agent : AggregateRoot
{
    public string Name { get; private set; } = ""; public string Icon { get; private set; } = ""; public AgentTier Tier { get; private set; } = AgentTier.WRK;
    public string Color { get; private set; } = "#6b7280"; public string Department { get; private set; } = ""; public string Description { get; private set; } = "";
    public string Skills { get; private set; } = "[]"; public string SystemPrompt { get; private set; } = ""; public bool IsBase { get; private set; } = true;
    public bool IsActive { get; private set; } = true;
    public bool IsImmortal { get; private set; } = false;
    public string Tools { get; private set; } = "[]";
    public string CreatedBy { get; private set; } = "system";
    public DateTime? RetiredAt { get; private set; }
    public string? ModelOverride { get; private set; }

    public void Retire() { IsActive = false; RetiredAt = DateTime.UtcNow; }
    public void Activate() { IsActive = true; RetiredAt = null; }
    public void SetImmortal() { IsImmortal = true; }

    private Agent() { }
    public static Agent Create(string id, string name, string icon, AgentTier tier, string color,
        string dept, string desc, string skills, string prompt, bool isBase = true,
        bool isImmortal = false, string tools = "[]", string createdBy = "system")
        => new()
        {
            Id = id, Name = name, Icon = icon, Tier = tier, Color = color,
            Department = dept, Description = desc, Skills = skills, SystemPrompt = prompt,
            IsBase = isBase, IsImmortal = isImmortal, Tools = tools, CreatedBy = createdBy
        };
}
