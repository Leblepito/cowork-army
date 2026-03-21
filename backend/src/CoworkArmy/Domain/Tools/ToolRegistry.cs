namespace CoworkArmy.Domain.Tools;

public class ToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();

    public void Register(ITool tool) => _tools[tool.Name] = tool;
    public ITool? Get(string name) => _tools.GetValueOrDefault(name);
    public IReadOnlyList<ITool> GetAll() => _tools.Values.ToList();

    public IReadOnlyList<ITool> GetForAgent(string[] allowedToolNames)
    {
        if (allowedToolNames.Length == 0)
            return GetAll().Where(t => t.Permission == ToolPermission.Safe).ToList();
        return allowedToolNames
            .Select(name => Get(name))
            .Where(t => t != null)
            .Cast<ITool>()
            .ToList();
    }
}
