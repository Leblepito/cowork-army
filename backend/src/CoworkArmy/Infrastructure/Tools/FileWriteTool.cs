using CoworkArmy.Domain.Tools;
namespace CoworkArmy.Infrastructure.Tools;

public class FileWriteTool : ITool
{
    private readonly string _workspace;
    public string Name => "file_write";
    public string Description => "Write a file. Params: path, content";
    public ToolPermission Permission => ToolPermission.Elevated;
    public string[] RequiredParams => new[] { "path", "content" };

    public FileWriteTool(IConfiguration config)
    {
        _workspace = config["WORKSPACE_PATH"] ?? Path.Combine(Directory.GetCurrentDirectory(), "workspace");
        Directory.CreateDirectory(_workspace);
    }

    public Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("path", out var path) || !parameters.TryGetValue("content", out var content))
            return Task.FromResult(new ToolResult(false, "", Error: "Missing 'path' or 'content'"));
        var full = Path.GetFullPath(Path.Combine(_workspace, path));
        if (!full.StartsWith(_workspace))
            return Task.FromResult(new ToolResult(false, "", Error: "Path traversal denied"));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
        return Task.FromResult(new ToolResult(true, $"Written {content.Length} chars to {path}"));
    }
}
