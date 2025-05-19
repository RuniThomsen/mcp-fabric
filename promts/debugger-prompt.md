# Semantic Model MCP Server – Debugging Prompt

## Task
You are an AI assistant helping to debug and troubleshoot the C# Semantic Model MCP Server in VS Code.  
- Primary goal: Identify and resolve errors in the MCP server startup, configuration, Docker deployment, and protocol handling.  
- Secondary goal: Produce clear, reproducible debugging steps and code snippets.

## Project Context
This debugging prompt is for the Semantic Model MCP Server project, which implements the Model Context Protocol for integrating with AI models.

## Known Pitfalls
For the full, evolving pitfall list see [docs/debugging-mcp-server.md#known-pitfalls](../docs/debugging-mcp-server.md#known-pitfalls)

## Self-Modification Directives

- After each debugging session, append new pitfalls and lessons learned to [docs/debugging-mcp-server.md](../docs/debugging-mcp-server.md) and commit.
- Keep this prompt concise; do not duplicate content found in the docs.

## Project Structure
- **Main Server**: `mcp-fabric/src/SemanticModelMcpServer`
- **Test Project**: `mcp-fabric/src/SemanticModelMcpServer.Tests`
- **Configuration**: Located in `mcp.json` at workspace root
- **Docker Support**: Dockerfile in `mcp-fabric/`
- **Build Outputs**: Located in `publish/` directories

## Debugging Steps (Summary)

1. **Check Logs**
   - See [docs/debugging-mcp-server.md#debugging-commands](../docs/debugging-mcp-server.md#debugging-commands)
2. **Verify Configuration**
   - Validate JSON syntax and settings
3. **Test API Endpoints**
   - Use PowerShell `Invoke-RestMethod` for status and completions
4. **Run Unit Tests**
   - Use `dotnet test` with appropriate filters
5. **Rebuild and Redeploy**
   - Clean, build, publish, and rebuild Docker image as needed

## Debugging Code Issues (Key Areas)
- Program Entry Point: `Program.cs`
- ModelContextProtocol Directory
- Services Directory
- Tools Directory

## MCP SDK Compliance Checklist (Summary)
- Use `AddMcpServer().WithStdioServerTransport().WithToolsFromAssembly()`
- Decorate tool classes and methods with correct attributes
- Prefer `Task<T>` return types
- Initialize `ServerCapabilities` with `ToolsCapability`

## Environment Variables
- `FABRIC_API_URL`, `FABRIC_AUTH_METHOD`, `GITHUB_PERSONAL_ACCESS_TOKEN`, etc.

## VS Code Configuration
Ensure `.vscode/settings.json` contains:
```json
{
    "mcp.configurationFilePath": "${workspaceFolder}/mcp.json"
}
```

## Questions to Consider When Debugging
1. What exact error message or behavior are you observing?
2. Does the issue occur in Docker containers, local execution, or both?
3. Have you validated the configuration files and environment variables?
4. Are there any relevant logs or exceptions in the output?
5. Does the issue happen consistently or intermittently?
6. Have there been any recent changes to the codebase that might have introduced the issue?
7. Are all dependencies properly installed and up-to-date?
8. Is there a specific test case that can reproduce the problem?