# Semantic Model MCP Server – TODO Prompt

This file tracks current issues and tasks for the Semantic Model MCP Server project. For detailed debugging and workflow instructions, always refer to [debugger-prompt.md](./debugger-prompt.md).

---

## How to Use
- Before starting work on any item, read and follow [debugger-prompt.md](./debugger-prompt.md).
- When you finish a task, remove it from this list and document any new pitfalls or lessons learned in [docs/debugging-mcp-server.md](../docs/debugging-mcp-server.md).

---

## Open Issues / Tasks

- [x] Fix MCP server process not exiting cleanly in DiagnosticsStreamTests (see recent test failures) - Fixed May 19, 2025
- [x] Add regression test for Docker startup with invalid mcp.json - Fixed May 19, 2025
- [x] Review and update environment variable documentation in README - Fixed May 19, 2025
- [x] Ensure all diagnostic output is routed to stderr (audit all Console.WriteLine usage) - Fixed May 19, 2025
- [x] Add more integration tests for protocol handshake edge cases - Fixed May 19, 2025
- [x] Ensure debugger-prompt.md and server implementation strictly follow Model Context Protocol C# SDK compliance patterns - Fixed May 19, 2025
    - Reference and mirror the bootstrap sequence and tool registration as in [EverythingServer sample](https://github.com/modelcontextprotocol/csharp-sdk/tree/main/samples/EverythingServer)
    - Cross-check compliance with the [SDK README](https://github.com/modelcontextprotocol/csharp-sdk/blob/main/README.md)
    - Validate mcp.json and server startup logic against the sample's minimal schema and startup flow
    - Require a validation step that confirms listTools returns ≥1 tool before running further tests

---

*For debugging steps, troubleshooting, and project structure, see [debugger-prompt.md](./debugger-prompt.md).*
