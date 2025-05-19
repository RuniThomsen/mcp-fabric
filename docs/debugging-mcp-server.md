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
- Added diagnostic tool to verify tool class registrations for MCP protocol compliance
- Fixed StdioServerTransport implementation to properly handle tool registration
- Enhanced logging verbosity for better debugging of tool discovery issues
- Added explicit tool registration in capabilities configuration for better tool surfacing
- Created comprehensive MCP server diagnostics tool to identify configuration issues
- Fixed discrepancy between Docker container port (80) and server port (8080)
- Added diagnostic tool to verify tool class registrations for MCP protocol compliance
- Fixed StdioServerTransport implementation to properly handle tool registration
- Enhanced logging verbosity for better debugging of tool discovery issues

### May 19, 2025
- **MCP Server Registration**: Fixed incorrect tool registration by replacing explicit tool registration with `WithToolsFromAssembly()`. This ensures all tools with proper attributes are automatically discovered and registered.
- **ServerCapabilities Configuration**: Fixed the `ToolsCapability` initialization by properly setting up `new ServerCapabilities { Tools = new ToolsCapability() }` instead of using reflection.
- **Port Availability Handling**: Enhanced port checking to exit the application when port 8080 is already in use, rather than just logging a warning which would lead to binding failures.
- **Configuration Validation**: Improved `mcp.json` validation to check for required fields and exit if the file is missing or contains invalid JSON.
- **Diagnostic Tool Timeout**: Added proper timeout handling to the `ToolSurfacingDiagnostics` to prevent the server from hanging indefinitely during tool registration checks. Implemented a 10-second timeout to ensure diagnostics complete gracefully.
- **SDK Compliance Verification**: Created a comprehensive SDK compliance verification tool that validates proper tool registration, method signatures, and server configuration to ensure compatibility with the MCP SDK.
- **Docker Image Availability**: Fixed server startup failure due to missing Docker image. Updated the diagnostics to check if the required Docker image (`mcp-server:latest`) exists and to build it automatically if not present. This prevents the "Error response from daemon: No such image" failure when starting the server.
- **Configuration File Mounting**: Fixed server startup failure due to the container not finding the `mcp.json` configuration file. Added logic to locate the configuration file (either in workspace root or in .vscode directory), copy it to the correct location, and properly mount it into the Docker container. This prevents the "ERROR: Configuration file mcp.json not found" failure.
- **Simplified Configuration Approach**: Resolved persistent configuration issues by creating a simplified configuration file specifically for the MCP server that only contains the essential properties needed (`fabricApiUrl` and `authMethod`). This eliminates complex VS Code configuration dependencies and ensures proper server startup.
- **Enhanced Script Error Handling**: Fixed "spawn UNKNOWN" errors in VS Code tasks by implementing robust error handling in the `start-mcp-server.ps1` script. Added detailed tracing, explicit error checks, and improved debugging output to identify and resolve script execution failures.
- **VS Code Settings Configuration**: Resolved integration issues between VS Code MCP extension and server by ensuring `.vscode/settings.json` contains proper `mcp.configurationFilePath` setting. This ensures the extension can locate and use the correct configuration file.
- **Process Path Resolution**: Fixed path resolution issues in the startup script by using absolute paths and explicit diagnostic logging of all file paths. This prevents silent failures when scripts can't locate necessary files or directories.
- **Docker Command Visibility**: Improved Docker command debugging by displaying the full command string before execution, making it easier to diagnose container startup failures and configuration issues.
- **Container Health Monitoring**: Added health checks to the Docker container configuration to properly monitor MCP server health and ensure the container is restarted when the server becomes unresponsive.

### May 19, 2025 (Continued)
- **Dependency Security Vulnerability**: Identified moderate severity vulnerabilities in Azure.Identity package v1.10.4 during the rebuild process. Added a step in the build pipeline to scan for security advisories in dependencies and provide recommendations for updates.
- **Build Process Optimization**: Improved the clean-build-publish workflow by implementing a single PowerShell script that handles the entire process sequentially, ensuring dependencies and build artifacts are properly handled between steps.
- **Docker Image Rebuild**: Enhanced the Docker image rebuild process to properly handle the project structure and dependencies. The build process now includes proper error handling and status reporting, with clear identification of any security advisories in the dependencies.
- **PowerShell Command Execution Standards**: Implemented consistent PowerShell command execution standards for scripts, using semicolons (`;`) as command separators in PowerShell scripts rather than ampersands (`&&`) which are more appropriate for bash/CMD.

### New Pitfall: Tool Surfacing Diagnostic Timeout

**Issue Description**: The warning "Tool surfacing diagnostic timed out after 10 seconds" occurs during server startup.

**Reproduction Steps**:
1. Start the Semantic Model MCP Server.
2. Observe the logs for the warning message.

**Root Cause**: The diagnostic tool initialization exceeds the default timeout threshold.

**Solution**: Increase the timeout duration in the server configuration file (`mcp.json`) under the diagnostics section.

**Prevention**: Ensure all tools are optimized for initialization within the configured timeout.

---

### New Lesson Learned: Health Endpoint Stability

**Lesson**: Adding a lightweight HTTP health endpoint (`/health`) significantly improves Docker container stability by ensuring health checks remain green.
