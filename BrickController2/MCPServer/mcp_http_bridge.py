#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import sys
import io
import os
import json
import requests
from typing import Any, Dict, Tuple

# ---------- Force UTF-8 streams before any I/O ----------

def _force_utf8_streams():
    try:
        sys.stdin = io.TextIOWrapper(sys.stdin.buffer, encoding="utf-8", errors="replace", newline="")
        sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace", newline="")
        sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace", newline="")
    except Exception:
        try:
            sys.stdin.reconfigure(encoding="utf-8", errors="replace")
            sys.stdout.reconfigure(encoding="utf-8", errors="replace")
            sys.stderr.reconfigure(encoding="utf-8", errors="replace")
        except Exception:
            pass

_force_utf8_streams()

# ---------- Config ----------

DEFAULT_PROTOCOL_VERSION = "2025-06-18"
DEFAULT_BASE_URL = "http://localhost:5000"
TIMEOUT = 15  # seconds

# ---------- Utils ----------

def log(msg: str):
    print(msg, file=sys.stderr, flush=True)

def send(obj: Dict[str, Any]):
    sys.stdout.write(json.dumps(obj, ensure_ascii=False) + "\n")
    sys.stdout.flush()

def error_response(id_, code=-32603, message="Internal error", data=None):
    err = {"code": code, "message": message}
    if data is not None:
        err["data"] = data
    return {"jsonrpc": "2.0", "id": id_, "error": err}

# ---------- Arg parsers ----------

def parse_base_url() -> str:
    args = sys.argv[1:]
    url = None
    for i, a in enumerate(args):
        if a in ("--url", "-u") and i + 1 < len(args):
            url = args[i + 1]
            break
    if not url:
        url = os.environ.get("MCP_HTTP_BASE_URL", DEFAULT_BASE_URL)
    return url.rstrip("/")

def parse_auth_token() -> str:
    args = sys.argv[1:]
    token = None
    for i, a in enumerate(args):
        if a in ("--token", "-t") and i + 1 < len(args):
            token = args[i + 1]
            break
    if not token:
        token = os.environ.get("MCP_HTTP_AUTH_TOKEN", "")
    return token

# ---------- HTTP session ----------

BASE_URL = parse_base_url()
AUTH_TOKEN = parse_auth_token()

SESSION = requests.Session()
SESSION.headers.update({
    "Accept": "application/json",
    "Content-Type": "application/json; charset=utf-8",
    "User-Agent": "windows-mcp-http-bridge/0.1.0"
})
if AUTH_TOKEN:
    SESSION.headers["Authorization"] = f"Bearer {AUTH_TOKEN}"

# ---------- HTTP helpers ----------

def _json_get(path: str) -> Dict[str, Any]:
    url = f"{BASE_URL}{path}"
    r = SESSION.get(url, timeout=TIMEOUT)
    r.raise_for_status()
    r.encoding = "utf-8"
    return r.json()

def _json_post(path: str, payload: Dict[str, Any]) -> Tuple[bool, Dict[str, Any]]:
    url = f"{BASE_URL}{path}"
    r = SESSION.post(url, json=payload, timeout=TIMEOUT)
    try:
        r.encoding = "utf-8"
        data = r.json()
    except Exception:
        data = {"error": "invalid_json_response", "status": r.status_code, "body": r.text}
    if r.status_code >= 400:
        return False, data if isinstance(data, dict) else {"status": r.status_code, "body": r.text}
    return True, data if isinstance(data, dict) else {}

def get_capabilities() -> Dict[str, Any]:
    return _json_get("/mcp/capabilities")

def execute_tool(name: str, arguments: Dict[str, Any]) -> Tuple[bool, Dict[str, Any]]:
    payload = {"name": name, "arguments": arguments or {}}
    return _json_post("/mcp/tools/execute", payload)

# ---------- MCP handlers ----------

def handle_initialize(msg: Dict[str, Any]):
    id_ = msg.get("id")
    result = {
        "protocolVersion": DEFAULT_PROTOCOL_VERSION,
        "capabilities": {},
        "serverInfo": {"name": "windows-mcp-http-bridge", "version": "0.1.0"},
    }
    send({"jsonrpc": "2.0", "id": id_, "result": result})

def handle_tools_list(msg: Dict[str, Any]):
    id_ = msg.get("id")
    try:
        caps = get_capabilities()
        tools_http = caps.get("tools", []) if isinstance(caps, dict) else []
        tools = []
        for t in tools_http:
            tools.append({
                "name": t.get("name"),
                "description": t.get("description"),
                "inputSchema": t.get("parameters") or {"type": "object", "properties": {}}
            })
        result = {"tools": tools}
        send({"jsonrpc": "2.0", "id": id_, "result": result})
    except Exception as e:
        log(f"[tools/list] Error: {e}")
        send(error_response(id_, message="Failed to fetch tools", data=str(e)))

def handle_tools_call(msg: Dict[str, Any]):
    id_ = msg.get("id")
    params = msg.get("params") or {}
    name = params.get("name") or params.get("toolName")
    arguments = params.get("arguments") or params.get("args") or {}

    if not name:
        send(error_response(id_, code=-32602, message="Missing tool name"))
        return

    try:
        ok, data = execute_tool(name, arguments)
        if ok:
            message = None
            if isinstance(data, dict):
                output = data.get("output")
                if isinstance(output, dict):
                    message = output.get("message")
            if not message:
                message = json.dumps(data, ensure_ascii=False)
            result = {"content": [{"type": "text", "text": message}]}
            send({"jsonrpc": "2.0", "id": id_, "result": result})
        else:
            if isinstance(data, dict):
                text = data.get("message") or data.get("error") or json.dumps(data, ensure_ascii=False)
            else:
                text = str(data)
            send(error_response(id_, code=-32000, message="Tool execution failed", data=text))
    except Exception as e:
        log(f"[tools/call] Error: {e}")
        send(error_response(id_, message="Exception during tool call", data=str(e)))

def handle_shutdown(msg: Dict[str, Any]):
    id_ = msg.get("id")
    send({"jsonrpc": "2.0", "id": id_, "result": None})
    sys.exit(0)

# ---------- Main loop ----------

def main():
    log(f"[bridge] Connecting to {BASE_URL}")
    if AUTH_TOKEN:
        log("[bridge] Using Bearer token authentication")
    for raw in sys.stdin:
        line = raw.strip()
        if not line:
            continue
        try:
            msg = json.loads(line)
        except Exception as e:
            log(f"[parse] Invalid JSON: {e} :: {line[:200]}")
            continue

        method = msg.get("method")
        if not method:
            continue

        if method == "initialize":
            handle_initialize(msg)
        elif method == "tools/list":
            handle_tools_list(msg)
        elif method == "tools/call":
            handle_tools_call(msg)
        elif method == "shutdown":
            handle_shutdown(msg)
        else:
            id_ = msg.get("id")
            if id_ is not None:
                send(error_response(id_, code=-32601, message=f"Method not found: {method}"))

if __name__ == "__main__":
    main()
