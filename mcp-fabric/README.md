# MCP Server for Power BI Semantic Model Development

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/)
[![Docker](https://img.shields.io/badge/docker-latest-blue)](https://hub.docker.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)

## Overview

The Model Context Protocol (MCP) server is designed to facilitate the development and management of Power BI Semantic Models via the Microsoft Fabric REST API. This project provides a comprehensive toolkit for creating, updating, refreshing, and validating semantic models, enabling seamless integration with Power BI workflows.

Built with cross-platform compatibility in mind, this MCP server supports both x64 and ARM64 architectures, making it versatile for various deployment scenarios. It leverages the latest .NET technologies to provide a robust and efficient interface to Microsoft Fabric's semantic model capabilities.

## Features

- **Create Semantic Model**: Easily create new semantic models with specified properties.
- **Update Semantic Model**: Update existing models with new definitions and configurations.
- **Refresh Semantic Model**: Trigger refresh operations to ensure models are up-to-date.
- **Validate TMDL**: Validate Tabular Model Definition Language (TMDL) files to ensure compliance.
- **Deployment Tools**: Deploy semantic models to various environments with ease.

## Project Structure

```
mcp-fabric
├── src
│   ├── SemanticModelMcpServer
│   │   ├── SemanticModelMcpServer.csproj
│   │   ├── Program.cs
│   │   ├── Tools
│   │   │   ├── CreateSemanticModelTool.cs
│   │   │   ├── UpdateSemanticModelTool.cs
│   │   │   ├── RefreshTool.cs
│   │   │   ├── ValidateTmdlTool.cs
│   │   │   └── DeploymentTool.cs
│   │   ├── Services
│   │   │   ├── FabricClient.cs
│   │   │   ├── ZipHelper.cs
│   │   │   ├── PbiToolsRunner.cs
│   │   │   └── TabularEditorRunner.cs
│   │   └── Models
│   │       ├── Requests
│   │       │   ├── CreateSemanticModelRequest.cs
│   │       │   ├── UpdateSemanticModelRequest.cs
│   │       │   └── RefreshSemanticModelRequest.cs
│   │       └── Responses
│   │           ├── CreateSemanticModelResponse.cs
│   │           ├── UpdateSemanticModelResponse.cs
│   │           └── RefreshSemanticModelResponse.cs
│   └── SemanticModelMcpServer.Tests
│       ├── SemanticModelMcpServer.Tests.csproj
│       ├── FabricClientTests.cs
│       └── ZipHelperTests.cs
├── Dockerfile
├── .gitignore
├── README.md
└── .github
    └── workflows
        ├── ci.yml
        └── cd.yml
```

## Getting Started

### Prerequisites

- .NET SDK 8.0 or later
- Docker (for containerization) - Supports both x64 and ARM64 architectures
- Access to Microsoft Fabric REST API
- Power BI tools for TMDL validation and processing
- Azure Authentication for secure access to Microsoft Fabric resources

### VS Code One-Click Setup

Quickly launch the repository in a ready-to-code environment:

[![Open in VS Code](https://img.shields.io/badge/Open%20in-VS%20Code-007ACC?logo=visualstudiocode&logoColor=white&style=for-the-badge)](https://open.vscode.dev/<org>/mcp-fabric)
[![Open in VS Code Docker](https://img.shields.io/badge/VS%20Code-Docker-2496ED?logo=docker&logoColor=white&style=for-the-badge)](vscode://ms-vscode-remote.remote-containers/openRepoInContainer?url=https%3A%2F%2Fgithub.com%2F<org>%2Fmcp-fabric)
[![Open in Dev Container](https://img.shields.io/badge/Dev%20Container-Docker-2496ED?logo=docker&logoColor=white&style=for-the-badge)](https://open.vscode.dev/<org>/mcp-fabric?feature=devcontainer)
[![Open in GitHub Codespaces](https://img.shields.io/badge/Open%20in-GitHub%20Codespaces-181717?logo=github&logoColor=white&style=for-the-badge)](https://github.com/codespaces/new?hide_repo_select=true&ref=main&repo=<org>/mcp-fabric)

Click any badge:

1. **Open in VS Code** – opens the repo in vscode.dev (browser).  
2. **VS Code Docker** – clones the repo and opens it directly inside the local Dev Container using Docker (VS Code Desktop + *Dev Containers* extension required).  
3. **Dev Container** – clones the repo locally and reopens it inside the pre-configured `.devcontainer` using the Docker backend (requires VS Code with the *Dev Containers* extension).  
4. **GitHub Codespaces** – opens the repository in a cloud-based development environment with a fully configured development container.

> Note: Replace `<org>` with your GitHub organization or user if you fork the repo.

### Docker Quick Launch

Quickly build and run the MCP server in Docker:

[![Quick Launch Docker](https://img.shields.io/badge/Quick%20Launch-Docker-2496ED?logo=docker&logoColor=white&style=for-the-badge)](https://hub.docker.com/)

Click the badge or use the following commands:

```powershell
# Build the Docker image
docker build -t mcp-server:latest .

# Run the container
docker run -p 8080:80 -e FABRIC_API_URL=https://api.fabric.microsoft.com -e FABRIC_AUTH_METHOD=ServicePrincipal mcp-server:latest
```

> Note: Ensure Docker is installed and running on your system. You can configure environment variables as needed for your setup.

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/<org>/mcp-fabric.git
   cd mcp-fabric/mcp-server
   ```

2. Restore dependencies:
   ```
   dotnet restore src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
   ```

3. Build the project:
   ```
   dotnet build src/SemanticModelMcpServer/SemanticModelMcpServer.csproj --configuration Release
   ```

4. Run the tests to verify your setup:
   ```
   dotnet test src/SemanticModelMcpServer.Tests/SemanticModelMcpServer.Tests.csproj
   ```

### Running the Server

To run the MCP server locally, execute the following command:
```
dotnet run --project src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
```

For development purposes, you can also run with environment variables:
```
$env:FABRIC_API_URL="https://api.fabric.microsoft.com" 
$env:FABRIC_AUTH_METHOD="ManagedIdentity"
dotnet run --project src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
```

### Docker

The project includes a Docker container that supports both x64 and ARM64 architectures. To build and run the Docker container:

```
# Build the Docker image
docker build -t mcp-server:latest .

# Run the container
docker run -p 8080:80 -e FABRIC_API_URL=https://api.fabric.microsoft.com -e FABRIC_AUTH_METHOD=ServicePrincipal mcp-server:latest
```

### VS Code + Docker (Dev Container)

You can develop, run, and debug the MCP server entirely inside Docker while coding in VS Code.

1. Install the *Dev Containers* extension (`ms-vscode-remote.remote-containers`).
2. Press `F1` → **Dev Containers: Reopen in Container**. VS Code will build the image defined in `.devcontainer/Dockerfile` (re-using the repo’s `Dockerfile`) and attach your editor to it.
3. Inside the container terminal, run:
   ```powershell
   dotnet run --project src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
   ```
4. Use the **Run & Debug** panel and launch profile *MCP Server (Docker)* to start/attach the debugger.

> The dev-container shares layers with the production image built above, so there is almost no additional disk or build overhead.

#### Alternative: attach to an already running container
```powershell
# Start container in background
Start-Process docker "run -d --name mcp-dev -p 8080:80 -e FABRIC_API_URL=https://api.fabric.microsoft.com -e FABRIC_AUTH_METHOD=ServicePrincipal mcp-server:latest"

# VS Code → F1 → “Dev Containers: Attach to Running Container…” → select mcp-dev
```

### Visual Studio Code Integration

The MCP server can be integrated with Visual Studio Code using the Model Context Protocol. For a one-click Docker launch, add the following configuration to your `settings.json`:

```jsonc
{
  "mcp": {
    "inputs": [
      {
        "type": "promptString",
        "id": "fabric_auth_method",
        "description": "Authentication method (ManagedIdentity | ServicePrincipal)",
        "default": "ManagedIdentity"
      }
    ],
    "servers": {
      "fabric": {
        "command": "docker",
        "args": [
          "run", "-i", "--rm",
          "-p", "8080:80",
          "-e", "FABRIC_API_URL=https://api.fabric.microsoft.com", // default URL (no prompt)
          "-e", "FABRIC_AUTH_METHOD=${input:fabric_auth_method}",
          "mcp/fabric:latest"
        ]
      }
    }
  }
}
```

1. Open *Preferences → Settings (JSON)* and paste the snippet.
2. Run **“MCP: Start Server”** and pick **fabric**.
3. VS Code prompts only for the authentication method, then pulls and starts the Docker image.

> The container is removed automatically after it stops (`--rm`). Adjust the port or additional environment variables as needed.

## Usage Examples

### Creating a Semantic Model

```csharp
// Create a request with TMDL files
var request = new CreateSemanticModelRequest
{
    Name = "MySemanticModel",
    Description = "A sample semantic model for Power BI",
    WorkspaceId = "your-workspace-id",
    TmdlFiles = new Dictionary<string, string>
    {
        { "model.tmdl", File.ReadAllText("path/to/model.tmdl") },
        { "tables/Customer.tmdl", File.ReadAllText("path/to/tables/Customer.tmdl") }
    }
};

// Execute the tool
var result = await createSemanticModelTool.ExecuteAsync(request);
Console.WriteLine($"Result: {result.Status}, Model ID: {result.ModelId}");
```

### Updating a Semantic Model

```csharp
// Update an existing semantic model
var updateRequest = new UpdateSemanticModelRequest
{
    ModelId = "existing-model-id",
    TmdlFiles = new Dictionary<string, string>
    {
        { "model.tmdl", File.ReadAllText("path/to/updated/model.tmdl") },
        { "tables/Customer.tmdl", File.ReadAllText("path/to/updated/tables/Customer.tmdl") }
    }
};

var updateResult = await updateSemanticModelTool.ExecuteAsync(updateRequest);
Console.WriteLine($"Update Result: {updateResult.Status}");
```

### Validating TMDL Files

```csharp
// Validate TMDL files
var tmdlFiles = new Dictionary<string, string>
{
    { "model.tmdl", File.ReadAllText("path/to/model.tmdl") },
    { "tables/Customer.tmdl", File.ReadAllText("path/to/tables/Customer.tmdl") }
};

var validationResult = await validateTmdlTool.ValidateAsync(tmdlFiles);
if (validationResult.IsValid)
{
    Console.WriteLine("TMDL files are valid!");
}
else
{
    Console.WriteLine($"Validation Errors: {string.Join(", ", validationResult.Errors)}");
}
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**:
   - Verify that you have proper access to Microsoft Fabric resources
   - Ensure your authentication credentials are correctly set up

2. **Docker Issues on ARM**:
   - Make sure your Docker installation supports ARM64 architecture
   - Pull the appropriate image tags for your architecture

3. **Missing Dependencies**:
   - Run `dotnet restore` to ensure all dependencies are properly installed
   - Check that you have the required versions of all tools

4. **Docker Image Not Found Error**:
   - If you encounter `Error response from daemon: No such image: mcp-server:latest` when starting the server, it means the Docker image hasn't been built yet
   - Solution: Run `docker build -t mcp-server:latest ./mcp-fabric/` to build the image before starting the server
   - Alternatively, use the provided script: `./mcp-fabric/scripts/ensure-docker-image.ps1` which automatically checks and builds the image if it doesn't exist
   - VS Code users can use the "MCP Server: Ensure Docker Image and Run" task which handles this automatically

5. **Configuration File Not Found Error**:
   - If you see `ERROR: Configuration file mcp.json not found` when running the Docker container, it means the configuration file isn't properly mounted
   - Solution: Ensure you mount the configuration file into the container with `-v "/path/to/mcp.json:/app/mcp.json"`
   - Check that your mcp.json exists in either the workspace root or in the .vscode directory
   - The updated script `./mcp-fabric/scripts/ensure-docker-image.ps1` will automatically locate and properly mount the configuration file

### Automated Diagnostics

The MCP server includes several built-in diagnostic tools that run at startup:

```powershell
# Run the server with diagnostics output
./publish/SemanticModelMcpServer.exe --verbose

# Check Docker image existence and build if needed
./mcp-fabric/scripts/ensure-docker-image.ps1

# View diagnostic output from a running container
docker logs semantic-model | Select-String "Diagnostic"
```

For more detailed troubleshooting guides, see the [Debugging Guide](../docs/debugging-mcp-server.md).

## Security, Supply Chain & Best Practices

- Uses Azure Managed Identity or DefaultAzureCredential for authentication (no secrets in code)
- Reads FABRIC_API_URL from environment/configuration (not hardcoded)
- Container includes a HEALTHCHECK for runtime validation
- CI pipeline generates a Software Bill of Materials (SBOM) and runs Trivy for vulnerability scanning
- Follows MCP SDK best practices for tool registration (see Program.cs for comments)
- Uses IHttpClientFactory for resilient HTTP calls

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature-name`
5. Submit a pull request

Please make sure to update tests as appropriate and adhere to the existing coding style.

## CI/CD Pipeline

The project includes GitHub Actions workflows for continuous integration and deployment:

- **CI Pipeline**: Automatically builds and tests the application on push to main and pull requests
- **CD Pipeline**: Handles deployment to development, staging, and production environments

### Pipeline Structure

```yaml
# CI Pipeline (ci.yml)
- Build and compile code
- Run unit tests
- Run code quality checks
- Build Docker image

# CD Pipeline (cd.yml)
- Deploy to development environment
- Run integration tests
- Deploy to staging environment
- Run acceptance tests
- Deploy to production environment
```

## Security

This project follows security best practices:

- Uses Azure Managed Identity where possible
- Avoids secrets in code or configuration files
- Implements proper authentication flows for Microsoft Fabric API
- Regular security dependency scanning with GitHub Actions

## License

MIT License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Microsoft for providing the Fabric REST API.
- The open-source community for their contributions and support.