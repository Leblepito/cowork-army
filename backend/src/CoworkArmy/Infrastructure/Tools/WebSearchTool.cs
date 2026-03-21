using CoworkArmy.Domain.Tools;
namespace CoworkArmy.Infrastructure.Tools;

public class WebSearchTool : ITool
{
    private readonly HttpClient _http;
    public string Name => "web_search";
    public string Description => "Search the web. Params: query";
    public ToolPermission Permission => ToolPermission.Safe;
    public string[] RequiredParams => new[] { "query" };

    public WebSearchTool(IHttpClientFactory httpFactory) => _http = httpFactory.CreateClient();

    public async Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("query", out var query))
            return new ToolResult(false, "", Error: "Missing 'query'");
        try
        {
            var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString(query)}&format=json&no_html=1";
            var response = await _http.GetStringAsync(url);
            return new ToolResult(true, response.Length > 2000 ? response[..2000] + "..." : response);
        }
        catch (Exception ex) { return new ToolResult(false, "", Error: ex.Message); }
    }
}
