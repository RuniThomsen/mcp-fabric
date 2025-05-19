# Semantic Model MCP Server – Debugging Prompt

## Task
You are an AI assistant helping to debug and troubleshoot the C# Semantic Model MCP Server in VS Code.  
- Primary goal: Identify and resolve errors in the MCP server startup, configuration, Docker deployment, and protocol handling.  
- Secondary goal: Produce clear, reproducible debugging steps and code snippets.

## Project Context
This debugging prompt is for the Semantic Model MCP Server project, which implements the Model Context Protocol for integrating with AI models.

## Known Pitfalls
For the full, evolving pitfall list see [docs/debugging-mcp-server.md#known-pitfalls](../docs/debugging-mcp-server.md#known-pitfalls)

> **New Pitfall (2025-05-19):**
>
> **Issue:** If `docker stop` or `docker rm` for the `semantic-model` container writes to **stdout**, the string (e.g. `semantic-model\n`) can corrupt the JSON-RPC stream, causing VS Code to see zero tools (`Discovered 0 tools`).
>
> **Root Cause:** The server's stdout is reserved for JSON-RPC. Any stray output (like container names) breaks protocol parsing.
>
> **Solution:** Redirect or suppress all stdout from these docker commands (e.g. `$null = docker stop semantic-model 2>&1`). Only write diagnostics to **stderr**.
>
> **Prevention:** Always ensure all non-JSON output (diagnostics, logs, command results) is sent to stderr, not stdout, during server startup and container management.

> **New Pitfall (2025-05-19):**
>
> **Issue:** Tests may hang or fail when the `HealthProbeHostedService` tries to start an HTTP listener on port 8080 but gets "Access is denied" due to insufficient permissions.
>
> **Root Cause:** The health probe was trying to bind to `http://*:8080/health/` which requires admin privileges on Windows. When the binding fails with `HttpListenerException (5): Access denied`, the service was not handling the error gracefully, causing tests to hang.
>
> **Solution:** Modify the `HealthProbeHostedService` to:
>   1. Use `http://localhost:{port}/health/` instead of `http://*:{port}/health/`
>   2. Add proper exception handling for `HttpListenerException` with error code 5
>   3. Log a warning but allow the server to continue running even when the health probe cannot start
>   4. Track the listener state with an `_isListening` flag to prevent null reference exceptions in Dispose
>
> **Prevention:** Always implement graceful fallback behavior for non-critical services like health probes. Use try/catch blocks with specific exception types, and log warnings rather than failing when services that aren't essential to core functionality encounter permission issues.

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
6. **Run Compliance Quick Test**
   - Execute the PowerShell script in **Quick Compliance Test Script** to verify `listTools` and basic capabilities

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

### Bootstrap Sequence Reference
```csharp
// Typical Program.cs bootstrap pattern (mirrors EverythingServer sample)
Host.CreateDefaultBuilder(args)
    .ConfigureMcpServer(builder =>
    {
        builder.AddMcpServer()
               .WithStdioServerTransport()
               .WithToolsFromAssembly(typeof(Program).Assembly)
               .WithDefaultJsonSerializerOptions();
    })
    .Build()
    .Run();
```

### Server Validation Checklist
1. Confirm `mcp.json` exists, is valid JSON, and includes minimal required properties (`name`, `version`, `entryPoint`).
2. Start the server and execute `listTools`. Expect **≥ 1** tool in the response.
3. Verify `initialize` returns `ServerCapabilities` with a populated `tools` array.
4. Ensure all diagnostic output is written to **stderr**.

### Quick Compliance Test Script (PowerShell)
```powershell
# Start server in background and capture PID
Start-Process -FilePath "./publish/SemanticModelMcpServer.exe" -ArgumentList "--stdio" -PassThru | Tee-Object -Variable proc;

# Send listTools request; stop on failure
echo '{ "jsonrpc":"2.0","id":1,"method":"listTools" }' |
    & "$Env:ProgramFiles\nodejs\node.exe" ".\scripts\call-stdio.js" |
    ConvertFrom-Json | ForEach-Object {
        if (-not $_.result.tools.Count) {
            Write-Error "No tools returned"; Stop-Process -Id $proc.Id
        }
    };

# Clean up
Stop-Process -Id $proc.Id;
```

> The script follows PowerShell guidelines (semicolon separators) and halts if `listTools` returns zero tools.

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

## Diagnostics-Only Mode
To run the server in diagnostics-only mode without starting the full server:
```powershell
# Run local server in diagnostics-only mode
./publish/SemanticModelMcpServer.exe --diagnostics-only;

# Run Docker container in diagnostics-only mode
docker run --rm mcp-server:latest --diagnostics-only;
```

## PowerShell Command Execution Standards
- Use semicolons (`;`) as command separators in PowerShell scripts, not ampersands (`&&`)
- For conditional execution, use proper PowerShell syntax:
  ```powershell
  # Use this for conditional execution (instead of &&)
  if ($?) { command2 }
  
  # Use this for fallback execution (instead of ||)
  command1; if (-not $?) { command2 }
  ```