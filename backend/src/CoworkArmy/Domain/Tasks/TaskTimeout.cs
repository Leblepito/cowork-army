namespace CoworkArmy.Domain.Tasks;

public static class TaskTimeout
{
    public static readonly TimeSpan LlmCall = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan WebSearch = TimeSpan.FromSeconds(15);
    public static readonly TimeSpan ApiCall = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan CodeExecute = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan DbQuery = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan PureThinking = TimeSpan.FromSeconds(120);
    public static readonly TimeSpan Default = TimeSpan.FromSeconds(60);

    public static TimeSpan ForTool(string toolName) => toolName switch
    {
        "web_search" => WebSearch,
        "api_call" => ApiCall,
        "code_execute" => CodeExecute,
        "db_query" => DbQuery,
        _ => Default,
    };
}
