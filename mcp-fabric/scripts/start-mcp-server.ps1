#!/usr/bin/env pwsh
# File: d:\repos\mcp-fabric\scripts\start-mcp-server.ps1
# Script to start the MCP server in a proper way for VS Code

# Set error action to stop so that errors are caught
$ErrorActionPreference = "Stop"

# Enable verbose output for better debugging
<# # Set-PSDebug -Trace 1 #>

try {
    [Console]::Error.WriteLine("Starting MCP Server with proper configuration...")

    # Get script path and derive other paths
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    [Console]::Error.WriteLine("Script path: $scriptPath")

    $projectRoot = Split-Path -Parent $scriptPath
    [Console]::Error.WriteLine("Project root: $projectRoot")

    $workspaceRoot = Split-Path -Parent $projectRoot
    [Console]::Error.WriteLine("Workspace root: $workspaceRoot")

    # Check if port 8080 is already in use
    $portInUse = $null
    try {
        $portInUse = Get-NetTCPConnection -LocalPort 8080 -ErrorAction SilentlyContinue
    } catch {
        [Console]::Error.WriteLine("Warning: Unable to check port status. Will proceed anyway: $_")
    }

    $serverPort = 8080
    if ($portInUse) {
        [Console]::Error.WriteLine("WARNING: Port 8080 is already in use. Trying alternative port 8081...")
        
        # Check if port 8081 is available as an alternative
        $port8081InUse = $null
        try {
            $port8081InUse = Get-NetTCPConnection -LocalPort 8081 -ErrorAction SilentlyContinue
        } catch {
            [Console]::Error.WriteLine("Warning: Unable to check port 8081 status. Will proceed anyway: $_")
        }
        
        if ($port8081InUse) {
            Write-Error "ERROR: Both ports 8080 and 8081 are in use. The MCP server requires an available port."
            Write-Error "Please stop any services using these ports before starting the MCP server."
            exit 1
        } else {
            $serverPort = 8081
            [Console]::Error.WriteLine("Using alternative port $serverPort.")
        }
    } else {
        [Console]::Error.WriteLine("Port $serverPort is available.")
    }

    # Create a simplified configuration file
    $simplifiedConfigPath = Join-Path $projectRoot "server-config.json"
    [Console]::Error.WriteLine("Config will be created at: $simplifiedConfigPath")

    # Check for configuration files
    $workspaceMcpJsonPath = Join-Path $workspaceRoot "mcp.json"
    $vscodeMcpJsonPath = Join-Path $workspaceRoot ".vscode\mcp.json"
    $projectMcpJsonPath = Join-Path $projectRoot "mcp.json"

    [Console]::Error.WriteLine("Checking for configuration files:")
    [Console]::Error.WriteLine("Workspace mcp.json: $workspaceMcpJsonPath")
    [Console]::Error.WriteLine("VS Code mcp.json: $vscodeMcpJsonPath")
    [Console]::Error.WriteLine("Project mcp.json: $projectMcpJsonPath")

    # Default values
    $authMethod = "ManagedIdentity"
    $fabricApiUrl = "https://api.fabric.microsoft.com"    # Try to load configuration from any available config file
    $configPath = $null

    if (Test-Path $workspaceMcpJsonPath) {
        [Console]::Error.WriteLine("Found workspace mcp.json at $workspaceMcpJsonPath")
        $configPath = $workspaceMcpJsonPath
    } elseif (Test-Path $vscodeMcpJsonPath) {
        [Console]::Error.WriteLine("Found VS Code mcp.json at $vscodeMcpJsonPath")
        $configPath = $vscodeMcpJsonPath
    } elseif (Test-Path $projectMcpJsonPath) {
        [Console]::Error.WriteLine("Found project mcp.json at $projectMcpJsonPath")
        $configPath = $projectMcpJsonPath
    }

    if ($configPath) {
        try {
            $configContent = Get-Content -Raw -Path $configPath -ErrorAction Stop
            $mcpJson = $configContent | ConvertFrom-Json -ErrorAction Stop
            
            # Extract values from config
            if ($mcpJson.authMethod) {
                $authMethod = $mcpJson.authMethod
                [Console]::Error.WriteLine("Loaded authMethod: $authMethod")
            }
            
            if ($mcpJson.fabricApiUrl) {
                $fabricApiUrl = $mcpJson.fabricApiUrl
                [Console]::Error.WriteLine("Loaded fabricApiUrl: $fabricApiUrl")
            }              # Variable is properly assigned and used to log success
            [Console]::Error.WriteLine("Successfully loaded configuration from $configPath")
        } catch {
            Write-Error "Error parsing config file: $_"
            [Console]::Error.WriteLine("Will use default values")
        }
    } else {
        [Console]::Error.WriteLine("No configuration file found, using default values")
    }

    # Create a simplified configuration file
    $simplifiedConfig = @{
        fabricApiUrl = $fabricApiUrl
        authMethod = $authMethod
    }

    # Write the simplified config to disk
    try {
        $simplifiedConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $simplifiedConfigPath -ErrorAction Stop
        [Console]::Error.WriteLine("Created simplified configuration at $simplifiedConfigPath")
    } catch {
        Write-Error "ERROR: Failed to create simplified configuration: $_"
        exit 1
    }

    # Ensure Docker is running
    # Check Docker daemon connectivity, ignoring warnings
    [Console]::Error.WriteLine("Checking Docker daemon...")
    docker version > $null 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: Docker daemon not accessible. Please ensure Docker is running."
        exit 1
    }
    [Console]::Error.WriteLine("Docker daemon is running.")

    # Check if Docker image exists
    try {
        [Console]::Error.WriteLine("Checking for Docker image: mcp-server:latest")
        $imageExists = docker images mcp-server:latest --format "{{.Repository}}" | Select-String -Pattern "mcp-server" -Quiet
        
        if (-not $imageExists) {
            [Console]::Error.WriteLine("mcp-server:latest image not found. Building image now...")
            
            $dockerfilePath = Join-Path $projectRoot "Dockerfile"
            if (-not (Test-Path $dockerfilePath)) {
                Write-Error "ERROR: Dockerfile not found at $dockerfilePath"
                exit 1
            }
            
            docker build -t mcp-server:latest $projectRoot
            
            if ($LASTEXITCODE -ne 0) {
                throw "Failed to build Docker image. Please check the Dockerfile and build logs."
            }
            
            [Console]::Error.WriteLine("Successfully built mcp-server:latest Docker image.")
        } else {
            [Console]::Error.WriteLine("mcp-server:latest Docker image already exists.")
        }
    } catch {
        Write-Error "ERROR: Docker image check failed: $_"
        exit 1
    }

    # Stop and remove existing container if it exists
    try {
        $containerExists = docker ps -a --filter "name=semantic-model" --format "{{.Names}}" | Select-String -Pattern "semantic-model" -Quiet
        if ($containerExists) {
            [Console]::Error.WriteLine("semantic-model container already exists. Stopping and removing...")
            docker stop semantic-model 2>$null
            docker rm semantic-model 2>$null
            
            if ($LASTEXITCODE -ne 0) {
                [Console]::Error.WriteLine("WARNING: Failed to remove existing container. This might cause issues.")
                [Console]::Error.WriteLine("Try running 'docker rm -f semantic-model' manually if you encounter problems.")
            } else {
                [Console]::Error.WriteLine("Successfully removed existing container.")
            }
        }
    } catch {
        [Console]::Error.WriteLine("WARNING: Failed to check/remove existing container: $_")
    }

    # Prepare environment variables
    $envVars = @(
        "-e", "MCP_DIAGNOSTIC_TIMEOUT=30000",
        "-e", "FABRIC_AUTH_METHOD=$authMethod",
        "-e", "FABRIC_API_URL=$fabricApiUrl"
    )

    # Make sure to use forward slashes for Docker path mapping on Windows
    $escapedConfigPath = $simplifiedConfigPath.Replace('\', '/')
    [Console]::Error.WriteLine("Using config file at: $escapedConfigPath")    # Build the Docker run command
    $dockerArgs = @(
        "run", "-i", "--rm",
        "-p", "${serverPort}:8080",
        "--name", "semantic-model",
        "--health-cmd", "curl -f http://localhost:8080/health || exit 1",
        "--health-interval", "10s",
        "--health-timeout", "5s",
        "--health-retries", "3",
        "--health-start-period", "10s",
        "-v", "${escapedConfigPath}:/app/mcp.json"
    )
    $dockerArgs += $envVars
    $dockerArgs += "mcp-server:latest"

    # Display the full command that will be run
    $cmdString = "docker " + ($dockerArgs -join " ")
    [Console]::Error.WriteLine("Running command: $cmdString")

    # Start the Docker container
    & docker $dockerArgs    # If we get here and the Docker command failed, exit with an error
    if ($LASTEXITCODE -ne 0) {
        Write-Error "ERROR: Docker container exited with code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
} catch {
    Write-Error "ERROR: An unexpected error occurred: $_"
    [Console]::Error.WriteLine("Stack trace: $($_.ScriptStackTrace)")
    exit 1
}
