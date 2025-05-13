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

#### Environment Variables for Docker

When running in Docker, you can configure the following environment variables:
- `FABRIC_API_URL`: URL of the Microsoft Fabric API
- `FABRIC_AUTH_METHOD`: Authentication method (`ManagedIdentity`, `ServicePrincipal`, or `Interactive`)
- `TENANT_ID`: Azure tenant ID (required for ServicePrincipal auth)
- `CLIENT_ID`: Client ID (required for ServicePrincipal auth)
- `CLIENT_SECRET`: Client secret (required for ServicePrincipal auth)

### Visual Studio Code Integration

The MCP server can be integrated with Visual Studio Code using the Model Context Protocol. Add the following configuration to your VS Code settings.json:

```json
"mcp": {
    "servers": {
        "fabric": {
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "${workspaceFolder}/src/SemanticModelMcpServer/SemanticModelMcpServer.csproj"
            ],
            "env": {
                "FABRIC_API_URL": "https://api.fabric.microsoft.com",
                "FABRIC_AUTH_METHOD": "Interactive"
            }
        }
    }
}
```

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