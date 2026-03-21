namespace CoworkArmy.Domain.Tools;

public interface ITool
{
    string Name { get; }
    string Description { get; }
    ToolPermission Permission { get; }
    string[] RequiredParams { get; }
    Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters);
}

public enum ToolPermission { Safe, Elevated, Dangerous }

public record ToolResult(bool Success, string Output, int TokensUsed = 0, string? Error = null);
