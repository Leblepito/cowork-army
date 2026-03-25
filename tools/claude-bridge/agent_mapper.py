"""Maps Claude Code tool calls to COWORK.ARMY agent IDs."""
from __future__ import annotations

AGENT_MAP: dict[str, str | dict[str, str] | None] = {
    "Edit": "full-stack",
    "Write": "full-stack",
    "Bash": {
        "test": "debugger",
        "pytest": "debugger",
        "vitest": "debugger",
        "git": "tech-lead",
        "build": "data-ops",
        "deploy": "data-ops",
        "npm": "data-ops",
        "dotnet": "data-ops",
        "default": "full-stack",
    },
    "Grep": "full-stack",
    "Glob": "full-stack",
    "Agent": "tech-lead",
    "Read": None,
}


def resolve_agent(tool: str, command: str | None = None) -> str | None:
    if tool not in AGENT_MAP:
        return "full-stack"
    mapping = AGENT_MAP[tool]
    if mapping is None:
        return None
    if isinstance(mapping, str):
        return mapping
    if isinstance(mapping, dict) and command:
        cmd_lower = command.lower()
        for keyword, agent_id in mapping.items():
            if keyword != "default" and keyword in cmd_lower:
                return agent_id
        return mapping.get("default", "full-stack")
    return "full-stack"


def resolve_summary(tool: str, file_path: str | None, command: str | None) -> str:
    if tool in ("Edit", "Write") and file_path:
        name = file_path.rsplit("/", 1)[-1] if "/" in file_path else file_path
        return f"{'Editing' if tool == 'Edit' else 'Writing'} {name}"
    if tool == "Bash" and command:
        short = command[:60] + ("..." if len(command) > 60 else "")
        return f"Running: {short}"
    if tool in ("Grep", "Glob"):
        return "Searching codebase"
    if tool == "Agent":
        return "Dispatching subagent"
    return f"{tool} operation"
