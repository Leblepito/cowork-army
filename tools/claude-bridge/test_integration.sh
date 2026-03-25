#!/usr/bin/env bash
set -euo pipefail

BRIDGE="${BRIDGE_URL:-http://localhost:8889}"
BACKEND="${BACKEND_URL:-http://localhost:8888}"

echo "=== Claude Bridge Integration Test ==="

echo -n "Bridge health... "
curl -sf "$BRIDGE/health" | grep -q '"status":"ok"' && echo "OK" || echo "FAIL"

echo -n "Hook (Edit)... "
RESP=$(curl -sf -X POST "$BRIDGE/hook" \
  -H "Content-Type: application/json" \
  -d '{"tool":"Edit","file":"test.ts","timestamp":"2026-03-25T12:00:00Z"}')
echo "$RESP" | grep -q '"forwarded\|debounced"' && echo "OK" || echo "FAIL: $RESP"

echo -n "Hook (Bash test)... "
RESP=$(curl -sf -X POST "$BRIDGE/hook" \
  -H "Content-Type: application/json" \
  -d '{"tool":"Bash","command":"npm test","timestamp":"2026-03-25T12:00:01Z"}')
echo "$RESP" | grep -q '"forwarded\|debounced"' && echo "OK" || echo "FAIL: $RESP"

echo -n "Hook (Read - silent)... "
RESP=$(curl -sf -X POST "$BRIDGE/hook" \
  -H "Content-Type: application/json" \
  -d '{"tool":"Read","file":"readme.md"}')
echo "$RESP" | grep -q '"silent"' && echo "OK" || echo "FAIL: $RESP"

echo -n "Task start... "
RESP=$(curl -sf -X POST "$BRIDGE/task/start" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test task","scope":"test","agents":["debugger"],"skill":"tdd"}')
echo "$RESP" | grep -q '"id\|Id"' && echo "OK (task created)" || echo "FAIL: $RESP"

echo -n "Backend events... "
curl -sf "$BACKEND/api/claude-bridge/events" | grep -q '\[' && echo "OK" || echo "FAIL"

echo "=== Done ==="
