#!/usr/bin/env pwsh
# Script to check and build the Docker image for MCP Server
# File: d:\repos\mcp-fabric\scripts\ensure-docker-image.ps1

Write-Host "Checking if mcp-server:latest Docker image exists..." -ForegroundColor Cyan

# Check if the Docker image exists
$imageExists = docker images mcp-server:latest --format "{{.Repository}}" | Select-String -Pattern "mcp-server" -Quiet

if (-not $imageExists) {
    Write-Host "mcp-server:latest image not found. Building image now..." -ForegroundColor Yellow
    
    # Get the directory of this script and navigate to the project root
    $scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
    $projectRoot = Split-Path -Parent $scriptPath
    
    # Build the Docker image
    docker build -t mcp-server:latest $projectRoot
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Successfully built mcp-server:latest Docker image." -ForegroundColor Green
    } else {
        Write-Host "Failed to build Docker image. Please check the Dockerfile and build logs." -ForegroundColor Red
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

# Ensure the config file exists
$workspaceRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
$configFile = Join-Path $workspaceRoot "mcp.json"
$vscodeConfigFile = Join-Path $workspaceRoot ".vscode\mcp.json"
$vscodeMcpConfig = Join-Path $workspaceRoot ".vscode\settings.json"

# Check if VS Code settings specify a custom location for mcp.json
$configPath = $configFile
if (Test-Path $vscodeMcpConfig) {
    $settings = Get-Content -Raw $vscodeMcpConfig | ConvertFrom-Json
    if ($settings.'mcp.configurationFilePath') {
        $customPath = $settings.'mcp.configurationFilePath' -replace '\$\{workspaceFolder\}', $workspaceRoot
        if (Test-Path $customPath) {
            $configPath = $customPath
            Write-Host "Using custom mcp.json path from VS Code settings: $configPath" -ForegroundColor Green
        }
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
        Write-Host "ERROR: mcp.json not found in any of the expected locations." -ForegroundColor Red
        Write-Host "Searched in:" -ForegroundColor Red
        Write-Host "  - $configFile" -ForegroundColor Red
        Write-Host "  - $vscodeConfigFile" -ForegroundColor Red
        Write-Host "  - Custom path from VS Code settings (if configured)" -ForegroundColor Red
        Write-Host "Please create a valid mcp.json configuration file." -ForegroundColor Red
        exit 1
    }
}

# Create a temporary copy of the config in the expected location for the container
$tempConfigFile = Join-Path $projectRoot "mcp.json"
Copy-Item -Path $configPath -Destination $tempConfigFile -Force
Write-Host "Copied configuration file from $configPath to $tempConfigFile for container mounting." -ForegroundColor Cyan

Write-Host "Docker image and container validation completed successfully." -ForegroundColor Cyan
Write-Host "Configuration file prepared for container." -ForegroundColor Cyan
exit 0
