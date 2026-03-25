# Claude Code Hook Configuration

Add to your Claude Code settings.json (`~/.claude/settings.json` or project settings):

## Hook: Post-tool event bridge

```json
{
  "hooks": {
    "PostToolUse": [
      {
        "matcher": "Edit|Write|Bash|Grep|Glob|Agent",
        "command": "curl -s -X POST http://localhost:8889/hook -H 'Content-Type: application/json' -d '{\"tool\":\"$TOOL_NAME\",\"file\":\"$FILE_PATH\",\"command\":\"$COMMAND\",\"timestamp\":\"'$(date -u +%FT%TZ)'\"}' > /dev/null 2>&1 &"
      }
    ]
  }
}
```

**Note:** The `&` at the end makes the curl fire-and-forget so it doesn't block Claude Code.
