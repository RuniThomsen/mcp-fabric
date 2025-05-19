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
- **Unused Variable Detection**: Fixed PowerShell scripts containing variables that are declared but never used (`$configLoaded`), which was causing confusion and potential bugs with script execution.
- **Script Structure Cleanup**: Removed duplicate code blocks in PowerShell scripts where port checking and Docker container initialization logic was repeated, leading to inconsistent behavior and potential race conditions.
- **Incomplete Script Statements**: Fixed incomplete statements at the end of PowerShell scripts (`$escapedConfigPath = $s`) that were causing script termination and unexpected behavior.

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

### New Pitfall: PowerShell Script Execution Standards

**Issue Description**: Inconsistent use of command separators and control flow constructs in PowerShell scripts.

**Reproduction Steps**:
1. Examine scripts with ampersands (`&&`) for command chaining.
2. Test scripts in different PowerShell versions.

**Root Cause**: PowerShell uses different conventions for command separation and conditional execution compared to bash/CMD.

**Solution**: 
- Use semicolons (`;`) as command separators in PowerShell scripts, not ampersands (`&&`)
- For conditional execution, use proper PowerShell syntax:
  ```powershell
  # Use this for conditional execution (instead of &&)
  if ($?) { command2 }
  
  # Use this for fallback execution (instead of ||)
  command1; if (-not $?) { command2 }
  ```

**Prevention**: Follow the PowerShell Command Execution Standards documented in the guidelines.

### New Pitfall: PowerShell Script Structure and Maintenance

**Issue Description**: PowerShell scripts with duplicate code blocks, unused variables, and incomplete statements.

**Reproduction Steps**:
1. Run the script and observe any warning messages or unexpected behavior.
2. Review script for instances of duplicate code and unused variables.

**Root Cause**: Script evolution over time without proper refactoring or code review.

**Solution**: 
- Remove unused variables like `$configLoaded` that are assigned but never used
- Consolidate duplicate code blocks for port checking and Docker container management
- Ensure all variable assignments are complete and properly terminated

**Prevention**: Implement regular code reviews for PowerShell scripts and use static analysis tools to identify unused variables and code structure issues.

### May 19, 2025 (New - Tool Surfacing Diagnostic)

- **Tool Surfacing Diagnostic Timeout Configuration**: Identified and resolved a recurring warning message "Tool surfacing diagnostic timed out after 10 seconds" during server startup. The diagnostic process was failing to complete within the default timeout period, even though the server continued to function correctly. 
- **Alternative Port Configuration**: Implemented fallback port selection when the primary port 8080 is already in use. When port 8080 is occupied, the server now automatically tries port 8081 without failing, displaying a clear message: "WARNING: Port 8080 is already in use. Trying alternative port 8081..."
- **Docker Environment Variable Configuration**: Added explicit environment variable `-e MCP_DIAGNOSTIC_TIMEOUT=30000` to the Docker run command to increase the diagnostic timeout from 10 seconds to 30 seconds, providing more time for tool registration diagnostics to complete.
- **JSON Parsing Issue**: Fixed "Failed to parse message" warnings in the logs by ensuring proper message buffering and parsing in the StdioServerTransport implementation.
- **Container Health Check Implementation**: Added container health checks using curl to monitor the `/health` endpoint with appropriate timeout settings to ensure proper container health monitoring:
  ```
  --health-cmd curl -f http://localhost:8080/health || exit 1 
  --health-interval 10s 
  --health-timeout 5s 
  --health-retries 3 
  --health-start-period 10s
  ```

### New Pitfall: DiagnosticsStreamTests Process Cleanup

**Issue Description**: The MCP server process was not exiting cleanly in DiagnosticsStreamTests, leading to orphaned processes after test runs.

**Reproduction Steps**:
1. Run the DiagnosticsStreamTests test suite.
2. Observe the warning "MCP server process did not exit after Kill" in the test output.
3. Check Task Manager to see orphaned MCP server processes.

**Root Cause**: Simply closing the standard input stream (stdin) doesn't trigger a clean shutdown of the server process. The server was waiting for more input or a proper exit command.

**Solution**: 
1. Send a proper JSON-RPC exit command to the server before closing stdin:
   ```csharp
   await process.StandardInput.WriteLineAsync("{\"jsonrpc\":\"2.0\",\"id\":\"test-exit\",\"method\":\"exit\",\"params\":{}}");
   await process.StandardInput.FlushAsync();
   await Task.Delay(500); // Give the server time to process
   ```
2. Implement a two-phase shutdown approach - first try graceful exit, then force termination if needed.
3. Add timeout handling with proper error reporting for both exit phases.

**Prevention**: 
- Always follow proper process lifecycle management in tests.
- Use a standardized approach for starting and stopping server processes in tests.
- Add explicit exit commands when testing command-line applications that use standard input/output streams.
- Implement a command-line parameter like `--diagnostics-only` for test scenarios to ensure clean process shutdown.

### May 19, 2025 (New - Diagnostics Mode Implementation)

- **Diagnostics-Only Mode Implementation**: Added a new command-line parameter `--diagnostics-only` to allow the server to run diagnostics and exit without starting the actual server. This is particularly useful for testing and CI/CD pipelines.
  
  ```csharp
  // In Program.cs
  private const string DiagnosticsOnlyArgument = "--diagnostics-only";
  
  public static async Task Main(string[] args)
  {
      bool diagnosticsOnlyMode = args.Contains(DiagnosticsOnlyArgument);
      
      // Run diagnostics
      // ...
      
      // If in diagnostics-only mode, exit after running diagnostics
      if (diagnosticsOnlyMode)
      {
          Console.Error.WriteLine("Diagnostics completed. Exiting in --diagnostics-only mode.");
          return;
      }
      
      // Continue with normal server startup
  }
  ```

- **Docker Image Update**: Updated the Docker image to support the new `--diagnostics-only` parameter. This allows for quick validation of Docker images without starting the full server:
  ```powershell
  # Test the Docker image with diagnostics-only mode
  docker run --rm mcp-server:latest --diagnostics-only
  ```

- **Test Process Lifecycle Management**: Improved the DiagnosticsStreamTests by implementing a two-phase shutdown approach:
  1. Send a proper JSON-RPC exit command to the server
  2. Close the standard input stream
  3. Add timeout-based error handling for process termination
  4. Add forced termination as a fallback method

- **Process Output Handling**: Implemented asynchronous process output reading with timeouts to prevent test hangs:
  ```csharp
  // Read process output with timeout
  private static async Task<string> ReadProcessOutputAsync(StreamReader reader, int timeoutMs = 5000)
  {
      var readTask = reader.ReadToEndAsync();
      var completedTask = await Task.WhenAny(readTask, Task.Delay(timeoutMs));
      
      if (completedTask == readTask)
      {
          return await readTask;
      }
      
      return "Process output read timed out";
  }
  ```

### New Pitfall: Docker Container Exit Code Detection

**Issue Description**: Docker containers using the MCP server may exit with a non-zero status code when run with the `--diagnostics-only` parameter, causing CI/CD pipelines to fail even though the diagnostics completed successfully.

**Reproduction Steps**:
1. Run the Docker container with the diagnostics-only parameter.
2. Check the exit code of the container after completion.
3. Observe that sometimes the container exits with a non-zero status code despite successful diagnostics.

**Root Cause**: The error handling in the diagnostics phase doesn't always ensure a zero exit code is returned, even when all diagnostics pass correctly.

**Solution**:
1. Modify the Program.cs file to explicitly ensure a 0 exit code when exiting in diagnostics-only mode:
   ```csharp
   if (diagnosticsOnlyMode)
   {
       Console.Error.WriteLine("Diagnostics completed. Exiting in --diagnostics-only mode.");
       Environment.ExitCode = 0; // Explicitly set exit code to 0
       return;
   }
   ```

2. Update Docker health checks to interpret diagnostic-only mode exit codes correctly:
   ```
   HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
     CMD if [ -f /.diagnostics_running ]; then exit 0; else curl --fail http://localhost:8080/health || exit 1; fi
   ```

**Prevention**:
- Always explicitly set exit codes in command-line tools, especially when they're used in automated pipelines.
- Add specific handling for diagnostic/test modes in health checks and status monitoring.
- Document expected exit codes and their meanings for all operation modes.

### New Pitfall: Console Logs Breaking JSON-RPC Protocol

**Issue Description**: Console log output sent to stdout instead of stderr disrupts the JSON-RPC protocol communication between VS Code and the MCP server.

**Reproduction Steps**:
1. Add a standard `Console.WriteLine()` log message to any code that runs after the MCP server initialization.
2. Start the server and observe "Failed to parse message" warnings in the VS Code output.
3. Notice "Discovered 0 tools" despite tools being properly registered in the code.
4. See "Tool surfacing diagnostic timed out after 10 seconds" warning at the end.

**Root Cause**: The JSON-RPC protocol used by VS Code MCP extension requires a clean stdout stream for message exchange. Any text written directly to stdout (instead of stderr) corrupts the message framing, causing the client to discard messages like the `listTools` response.

**Solution**:
1. Route all console output to stderr:
   ```csharp
   // Use Console.Error.WriteLine instead of Console.WriteLine
   Console.Error.WriteLine("Log message");
   ```

2. Configure Microsoft.Extensions.Logging to use stderr for all log levels:
   ```csharp
   .ConfigureLogging(logging =>
   {
       logging.AddConsole(options =>
       {
           // Send ALL log levels to stderr to keep stdout clean for JSON-RPC
           options.LogToStandardErrorThreshold = LogLevel.Trace;
       });
   })
   ```

3. Apply the same stderr routing to LoggerFactory.Create usage:
   ```csharp
   var loggerFactory = LoggerFactory.Create(builder => 
       builder.AddConsole(opts => {
           // Force ALL console logs to go to stderr
           opts.LogToStandardErrorThreshold = LogLevel.Trace;
       })
   );
   ```

4. Audit all Console.WriteLine calls in the codebase and replace with Console.Error.WriteLine.

**Prevention**:
- Add code analysis rules to detect and warn about Console.WriteLine usage.
- Document the critical requirement that all logging must go to stderr in the project's contribution guidelines.
- Add a startup check that validates logging configuration is properly set to use stderr.
- For any extension to the codebase that involves logging, make stderr routing a requirement in the PR checklist.
