"""Tests for agent_mapper module."""
from agent_mapper import resolve_agent, resolve_summary


def test_edit_maps_to_fullstack():
    assert resolve_agent("Edit") == "full-stack"

def test_write_maps_to_fullstack():
    assert resolve_agent("Write") == "full-stack"

def test_bash_test_maps_to_debugger():
    assert resolve_agent("Bash", "npm test") == "debugger"
    assert resolve_agent("Bash", "pytest tests/") == "debugger"

def test_bash_git_maps_to_techlead():
    assert resolve_agent("Bash", "git commit -m 'fix'") == "tech-lead"

def test_bash_build_maps_to_dataops():
    assert resolve_agent("Bash", "npm run build") == "data-ops"
    assert resolve_agent("Bash", "dotnet build") == "data-ops"

def test_bash_default_maps_to_fullstack():
    assert resolve_agent("Bash", "ls -la") == "full-stack"

def test_read_is_silent():
    assert resolve_agent("Read") is None

def test_agent_maps_to_techlead():
    assert resolve_agent("Agent") == "tech-lead"

def test_unknown_tool_maps_to_fullstack():
    assert resolve_agent("UnknownTool") == "full-stack"

def test_summary_edit():
    s = resolve_summary("Edit", "src/App.tsx", None)
    assert "App.tsx" in s

def test_summary_bash():
    s = resolve_summary("Bash", None, "npm test")
    assert "npm test" in s

def test_summary_grep():
    s = resolve_summary("Grep", None, None)
    assert "Searching" in s
