using CoworkArmy.Domain.Tools;
using CoworkArmy.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoworkArmy.Infrastructure.Tools;

public class DbQueryTool : ITool
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DbQueryTool> _log;

    public string Name => "db_query";
    public string Description => "Execute a read-only SQL SELECT query against the database. Params: sql";
    public ToolPermission Permission => ToolPermission.Elevated;
    public string[] RequiredParams => new[] { "sql" };

    public DbQueryTool(IServiceScopeFactory scopeFactory, ILogger<DbQueryTool> log)
    {
        _scopeFactory = scopeFactory;
        _log = log;
    }

    public async Task<ToolResult> ExecuteAsync(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("sql", out var sql) || string.IsNullOrWhiteSpace(sql))
            return new ToolResult(false, "", Error: "Missing 'sql' parameter");

        // Safety: only simple SELECT allowed
        var trimmed = sql.TrimStart().ToUpperInvariant();
        if (!trimmed.StartsWith("SELECT"))
            return new ToolResult(false, "", Error: "Only SELECT queries allowed");

        var blocked = new[] { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "TRUNCATE", "CREATE",
            "GRANT", "EXEC", "UNION", "INTO OUTFILE", "COPY", "pg_read_file", "pg_stat",
            "information_schema", "--", ";", "/*" };
        if (blocked.Any(b => trimmed.Contains(b, StringComparison.OrdinalIgnoreCase)))
            return new ToolResult(false, "", Error: "Query contains blocked keywords");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CoworkDbContext>();

            // Execute as raw SQL and collect results
            var results = new List<Dictionary<string, object?>>();
            await using var conn = db.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 5; // 5 second timeout for safety

            await using var reader = await cmd.ExecuteReaderAsync();
            var columnCount = reader.FieldCount;
            var columns = Enumerable.Range(0, columnCount).Select(reader.GetName).ToList();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < columnCount; i++)
                    row[columns[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                results.Add(row);

                if (results.Count >= 100) break; // Cap at 100 rows
            }

            _log.LogInformation("DbQuery executed: rows={Count}, sql={Sql}", results.Count, sql[..Math.Min(sql.Length, 100)]);

            var json = System.Text.Json.JsonSerializer.Serialize(results);
            return new ToolResult(true, $"Rows: {results.Count}\n{json}");
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DbQuery failed: {Sql}", sql);
            return new ToolResult(false, "", Error: $"Query failed: {ex.Message}");
        }
    }
}
