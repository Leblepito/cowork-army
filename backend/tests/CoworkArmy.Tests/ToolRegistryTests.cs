using CoworkArmy.Domain.Tools;
using Xunit;

namespace CoworkArmy.Tests;

public class ToolRegistryTests
{
    private class FakeTool : ITool
    {
        public string Name { get; init; } = "";
        public string Description => "";
        public ToolPermission Permission { get; init; }
        public string[] RequiredParams => Array.Empty<string>();
        public Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
            => Task.FromResult(new ToolResult(true, "ok"));
    }

    [Fact]
    public void Register_and_Get_returns_tool()
    {
        var registry = new ToolRegistry();
        var tool = new FakeTool { Name = "test" };
        registry.Register(tool);
        Assert.Equal(tool, registry.Get("test"));
    }

    [Fact]
    public void GetForAgent_empty_array_returns_only_Safe_tools()
    {
        var registry = new ToolRegistry();
        registry.Register(new FakeTool { Name = "safe1", Permission = ToolPermission.Safe });
        registry.Register(new FakeTool { Name = "elevated1", Permission = ToolPermission.Elevated });
        registry.Register(new FakeTool { Name = "dangerous1", Permission = ToolPermission.Dangerous });

        var tools = registry.GetForAgent(Array.Empty<string>());
        Assert.Single(tools);
        Assert.Equal("safe1", tools[0].Name);
    }

    [Fact]
    public void GetForAgent_with_names_returns_specified_tools()
    {
        var registry = new ToolRegistry();
        registry.Register(new FakeTool { Name = "a", Permission = ToolPermission.Dangerous });
        registry.Register(new FakeTool { Name = "b", Permission = ToolPermission.Safe });

        var tools = registry.GetForAgent(new[] { "a", "b" });
        Assert.Equal(2, tools.Count);
    }

    [Fact]
    public void Get_unknown_returns_null()
    {
        var registry = new ToolRegistry();
        Assert.Null(registry.Get("nonexistent"));
    }
}
