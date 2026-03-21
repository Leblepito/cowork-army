using System.Diagnostics;
using CoworkArmy.Domain.Tools;
namespace CoworkArmy.Infrastructure.Tools;

public class CodeExecuteTool : ITool
{
    private readonly ILogger<CodeExecuteTool> _log;
    public string Name => "code_execute";
    public string Description => "Execute Python code in sandbox. Params: code";
    public ToolPermission Permission => ToolPermission.Dangerous;
    public string[] RequiredParams => new[] { "code" };

    public CodeExecuteTool(ILogger<CodeExecuteTool> log) => _log = log;

    public async Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("code", out var code))
            return new ToolResult(false, "", Error: "Missing 'code'");
        var tmp = Path.GetTempFileName() + ".py";
        try
        {
            await File.WriteAllTextAsync(tmp, code);
            var psi = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "python" : "python3",
                Arguments = tmp,
                RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true,
            };
            using var proc = Process.Start(psi);
            if (proc == null) return new ToolResult(false, "", Error: "Failed to start Python");
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try { await proc.WaitForExitAsync(cts.Token); }
            catch (OperationCanceledException) { proc.Kill(true); return new ToolResult(false, "", Error: "Timed out (30s)"); }
            var stdout = await proc.StandardOutput.ReadToEndAsync();
            var stderr = await proc.StandardError.ReadToEndAsync();
            var output = stdout + (stderr.Length > 0 ? $"\nSTDERR: {stderr}" : "");
            if (output.Length > 10_000) output = output[..10_000] + "\n...(truncated)";
            _log.LogInformation("Code executed: exit={Code}", proc.ExitCode);
            return proc.ExitCode == 0 ? new ToolResult(true, output) : new ToolResult(false, output, Error: $"Exit {proc.ExitCode}");
        }
        finally { try { File.Delete(tmp); } catch { } }
    }
}
