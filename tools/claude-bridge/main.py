"""Claude Code -> COWORK.ARMY bridge."""
from __future__ import annotations

import asyncio
import os
import time
from collections import defaultdict
from contextlib import asynccontextmanager
from typing import Any

import httpx
from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse

from agent_mapper import resolve_agent, resolve_summary

COWORK_BACKEND_URL = os.getenv("COWORK_BACKEND_URL", "http://localhost:8888")
DEBOUNCE_MS = int(os.getenv("DEBOUNCE_MS", "500"))
PORT = int(os.getenv("PORT", "8889"))
MAX_QUEUE = 100

_retry_queue: list[dict[str, Any]] = []
_debounce_timers: dict[str, float] = defaultdict(float)
_debounce_events: dict[str, dict[str, Any]] = {}
_client: httpx.AsyncClient | None = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    global _client
    _client = httpx.AsyncClient(timeout=2.0)
    task = asyncio.create_task(_retry_loop())
    yield
    task.cancel()
    await _client.aclose()


app = FastAPI(title="Claude Bridge", version="1.0", lifespan=lifespan)


@app.get("/health")
async def health():
    return {"status": "ok", "backend": COWORK_BACKEND_URL, "queue_size": len(_retry_queue)}


@app.post("/hook")
async def receive_hook(request: Request):
    body = await request.json()
    tool = body.get("tool", "unknown")
    file_path = body.get("file")
    command = body.get("command")
    metadata = body.get("metadata", {})

    agent_id = resolve_agent(tool, command)
    if agent_id is None:
        return JSONResponse({"status": "silent", "tool": tool})

    summary = resolve_summary(tool, file_path, command)
    event_payload = {
        "tool": tool, "agentId": agent_id, "summary": summary,
        "filePath": file_path, "taskId": body.get("task_id"),
        "metadata": str(metadata),
    }

    now = time.monotonic()
    last = _debounce_timers.get(agent_id, 0)
    if (now - last) * 1000 < DEBOUNCE_MS:
        _debounce_events[agent_id] = event_payload
        _debounce_timers[agent_id] = now
        asyncio.create_task(_delayed_flush(agent_id, DEBOUNCE_MS / 1000.0))
        return JSONResponse({"status": "debounced", "agent": agent_id})

    _debounce_timers[agent_id] = now
    await _forward_event(event_payload)
    return JSONResponse({"status": "forwarded", "agent": agent_id})


@app.post("/task/start")
async def task_start(request: Request):
    body = await request.json()
    try:
        resp = await _client.post(f"{COWORK_BACKEND_URL}/api/claude-bridge/tasks/start", json=body)
        return JSONResponse(resp.json(), status_code=resp.status_code)
    except httpx.RequestError:
        return JSONResponse({"error": "backend unreachable"}, status_code=503)


@app.post("/task/complete")
async def task_complete(request: Request):
    body = await request.json()
    task_id = body.pop("task_id", None)
    if not task_id:
        return JSONResponse({"error": "task_id required"}, status_code=400)
    try:
        resp = await _client.post(f"{COWORK_BACKEND_URL}/api/claude-bridge/tasks/{task_id}/complete", json=body)
        return JSONResponse(resp.json(), status_code=resp.status_code)
    except httpx.RequestError:
        return JSONResponse({"error": "backend unreachable"}, status_code=503)


async def _delayed_flush(agent_id: str, delay: float) -> None:
    await asyncio.sleep(delay)
    ev = _debounce_events.pop(agent_id, None)
    if ev:
        await _forward_event(ev)


async def _forward_event(payload: dict[str, Any]) -> None:
    try:
        await _client.post(f"{COWORK_BACKEND_URL}/api/claude-bridge/events", json=payload)
    except httpx.RequestError:
        if len(_retry_queue) < MAX_QUEUE:
            _retry_queue.append(payload)


async def _retry_loop() -> None:
    while True:
        await asyncio.sleep(5)
        if not _retry_queue:
            continue
        batch = _retry_queue[:]
        _retry_queue.clear()
        for payload in batch:
            try:
                await _client.post(f"{COWORK_BACKEND_URL}/api/claude-bridge/events", json=payload)
            except httpx.RequestError:
                if len(_retry_queue) < MAX_QUEUE:
                    _retry_queue.append(payload)
                break


if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="127.0.0.1", port=PORT, reload=True)
