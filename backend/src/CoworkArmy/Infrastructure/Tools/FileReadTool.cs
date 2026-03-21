using CoworkArmy.Domain.Tools;
namespace CoworkArmy.Infrastructure.Tools;

public class FileReadTool : ITool
{
    private readonly string _workspace;
    public string Name => "file_read";
    public string Description => "Read a file. Params: path";
    public ToolPermission Permission => ToolPermission.Safe;
    public string[] RequiredParams => new[] { "path" };

    public FileReadTool(IConfiguration config)
    {
        _workspace = config["WORKSPACE_PATH"] ?? Path.Combine(Directory.GetCurrentDirectory(), "workspace");
        Directory.CreateDirectory(_workspace);
    }

    public Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("path", out var path))
            return Task.FromResult(new ToolResult(false, "", Error: "Missing 'path'"));
        var full = Path.GetFullPath(Path.Combine(_workspace, path));
        if (!full.StartsWith(_workspace))
            return Task.FromResult(new ToolResult(false, "", Error: "Path traversal denied"));
        if (!File.Exists(full))
            return Task.FromResult(new ToolResult(false, "", Error: $"File not found: {path}"));
        var content = File.ReadAllText(full);
        if (content.Length > 10_000) content = content[..10_000] + "\n...(truncated)";
        return Task.FromResult(new ToolResult(true, content));
    }
}
