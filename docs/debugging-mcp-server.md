# Semantic Model MCP Server Debugging Guide

## Project Context
This document contains debugging notes and lessons learned while troubleshooting the C# Semantic Model MCP Server. It serves as a knowledge base for common issues and their solutions to prevent recurrence.

## Date / Author
Initial creation: May 18, 2025 / GitHub Copilot

## Debugging Commands
For quick reference, these are the most useful debugging commands for the MCP Server:

```powershell
# Check if port 8080 is available
Test-NetConnection -ComputerName localhost -Port 8080;

# Validate mcp.json configuration
Get-Content -Path "mcp.json" -Raw | ConvertFrom-Json;

# View Docker container status
docker ps -a | Select-String "semantic-model";

# View container logs with error filtering
docker logs $(docker ps -q --filter "name=semantic-model") | Select-String -Pattern "ERROR|WARN";

# Run the server with verbose logging and token masking
./publish/SemanticModelMcpServer.exe --verbose --mask-tokens;

# Test the API endpoint
Invoke-RestMethod http://localhost:8080/status;
```

See the full debugging prompt at [promts/debugger-prompt.md](../promts/debugger-prompt.md) for more detailed commands.

## VS Code Integration
This project includes VS Code Tasks and Launch configurations in the `.vscode` folder:

- **Tasks**: Run common operations like diagnostics, build, test, and Docker operations
- **Launch Configurations**: Debug the server locally or attach to running processes

Run these tasks from the VS Code Command Palette (`Ctrl+Shift+P` → "Tasks: Run Task").

The following key tasks are available:
- **MCP Server: Run Diagnostics** - Checks port availability, config file, and Docker status
- **MCP Server: Watch Logs** - Monitors log files in real-time
- **MCP Server: Build and Run** - Compiles and starts the server
- **MCP Server: Run in Docker** - Builds and runs the server in a Docker container

For more details, see the [.vscode/tasks.json](../.vscode/tasks.json) and [.vscode/launch.json](../.vscode/launch.json) files.

## Lessons Learned

### May 18, 2025
- Fixed port binding race by checking port availability before dotnet run
- Added proper exception handling for configuration file loading to prevent silent failures
- Implemented token masking in logs to prevent sensitive information exposure
- Resolved Docker container networking issues by explicitly setting host network mode
- Fixed MCP protocol handling by ensuring proper content-type headers in responses
- Improved configuration validation to catch malformed JSON in mcp.json before startup
