#!/usr/bin/env pwsh
# Script to check and build the Docker image for MCP Server
# File: d:\repos\mcp-fabric\scripts\ensure-docker-image.ps1

Write-Host "Checking if mcp-server:latest Docker image exists..." -ForegroundColor Cyan

# Check if the Docker image exists
$imageExists = docker images mcp-server:latest --format "{{.Repository}}" | Select-String -Pattern "mcp-server" -Quiet

if (-not $imageExists) {
    Write-Host "mcp-server:latest image not found. Building image now..." -ForegroundColor Yellow
    
    # Get the directory of this script and navigate to the project root
    $scriptDir = $null
    try {
        $scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
        if (-not $scriptDir) {
            throw "Unable to determine script directory"
        }
        
        $projectRoot = Split-Path -Parent $scriptDir
        Write-Host "Script directory: $scriptDir" -ForegroundColor Cyan
        Write-Host "Project root: $projectRoot" -ForegroundColor Cyan
        
        # Build the Docker image
        docker build -t mcp-server:latest $projectRoot
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully built mcp-server:latest Docker image." -ForegroundColor Green
        } else {
            Write-Host "Failed to build Docker image. Please check the Dockerfile and build logs." -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "ERROR: $_" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "mcp-server:latest Docker image already exists." -ForegroundColor Green
}

# Check if the Docker container is running
$containerRunning = docker ps --filter "name=semantic-model" --format "{{.Names}}" | Select-String -Pattern "semantic-model" -Quiet

if ($containerRunning) {
    Write-Host "semantic-model container is already running. Stopping and removing..." -ForegroundColor Yellow
    docker stop semantic-model
    docker rm semantic-model
}

# Get directory paths
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
if (-not $scriptDir) {
    Write-Host "ERROR: Unable to determine script directory" -ForegroundColor Red
    exit 1
}

$projectRoot = Split-Path -Parent $scriptDir
$workspaceRoot = Split-Path -Parent $projectRoot

if (-not $projectRoot) {
    Write-Host "ERROR: Unable to determine project root directory" -ForegroundColor Red 
    exit 1
}

if (-not $workspaceRoot) {
    # Fall back to project root if workspace root can't be determined
    $workspaceRoot = $projectRoot
    Write-Host "WARNING: Unable to determine workspace root, using project root instead" -ForegroundColor Yellow
}

Write-Host "Script directory: $scriptDir" -ForegroundColor Cyan
Write-Host "Project root: $projectRoot" -ForegroundColor Cyan
Write-Host "Workspace root: $workspaceRoot" -ForegroundColor Cyan

# Ensure the config file exists
$configFile = Join-Path $workspaceRoot "mcp.json"
$vscodeConfigFile = Join-Path $workspaceRoot ".vscode\mcp.json"
$vscodeMcpConfig = Join-Path $workspaceRoot ".vscode\settings.json"

Write-Host "Looking for config files at:" -ForegroundColor Cyan
Write-Host "  - Workspace mcp.json: $configFile" -ForegroundColor Cyan
Write-Host "  - VS Code mcp.json: $vscodeConfigFile" -ForegroundColor Cyan
Write-Host "  - VS Code settings: $vscodeMcpConfig" -ForegroundColor Cyan

# Check if VS Code settings specify a custom location for mcp.json
$configPath = $configFile
if (Test-Path $vscodeMcpConfig) {
    try {
        $settings = Get-Content -Raw $vscodeMcpConfig | ConvertFrom-Json
        if ($settings.'mcp.configurationFilePath') {
            $customPath = $settings.'mcp.configurationFilePath' -replace '\$\{workspaceFolder\}', $workspaceRoot
            if (Test-Path $customPath) {
                $configPath = $customPath
                Write-Host "Using custom mcp.json path from VS Code settings: $configPath" -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "WARNING: Error reading VS Code settings: $_" -ForegroundColor Yellow
    }
}

# If custom path doesn't exist, fall back to standard locations
if (-not (Test-Path $configPath)) {
    if (Test-Path $vscodeConfigFile) {
        $configPath = $vscodeConfigFile
        Write-Host "Found mcp.json in .vscode directory." -ForegroundColor Green
    } elseif (Test-Path $configFile) {
        $configPath = $configFile
        Write-Host "Found mcp.json in workspace root." -ForegroundColor Green
    } else {
        # Create a default configuration if none exists
        Write-Host "WARNING: mcp.json not found in any of the expected locations." -ForegroundColor Yellow
        Write-Host "Creating a default configuration file." -ForegroundColor Yellow
        
        # Create a default configuration
        $defaultConfig = @{
            fabricApiUrl = "https://api.fabric.microsoft.com"
            authMethod = "ManagedIdentity"
        }
        
        $configPath = Join-Path $projectRoot "mcp.json"
        $defaultConfig | ConvertTo-Json -Depth 10 | Set-Content -Path $configPath
        Write-Host "Created default configuration at $configPath" -ForegroundColor Green
    }
}

# Create a temporary copy of the config in the expected location for the container
$tempConfigFile = Join-Path $projectRoot "mcp.json"
Copy-Item -Path $configPath -Destination $tempConfigFile -Force
Write-Host "Copied configuration file from $configPath to $tempConfigFile for container mounting." -ForegroundColor Cyan

Write-Host "Docker image and container validation completed successfully." -ForegroundColor Cyan
Write-Host "Configuration file prepared for container." -ForegroundColor Cyan
exit 0
