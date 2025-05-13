# MCP Server for Power BI Semantic Model Development

## Overview

The Model Context Protocol (MCP) server is designed to facilitate the development and management of Power BI Semantic Models via the Microsoft Fabric REST API. This project provides a comprehensive toolkit for creating, updating, refreshing, and validating semantic models, enabling seamless integration with Power BI workflows.

## Features

- **Create Semantic Model**: Easily create new semantic models with specified properties.
- **Update Semantic Model**: Update existing models with new definitions and configurations.
- **Refresh Semantic Model**: Trigger refresh operations to ensure models are up-to-date.
- **Validate TMDL**: Validate Tabular Model Definition Language (TMDL) files to ensure compliance.
- **Deployment Tools**: Deploy semantic models to various environments with ease.

## Project Structure

```
mcp-server
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
- Docker (for containerization)
- Access to Microsoft Fabric REST API

### Installation

1. Clone the repository:
   ```
   git clone https://github.com/<org>/<repo>.git
   cd mcp-server
   ```

2. Restore dependencies:
   ```
   dotnet restore src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
   ```

3. Build the project:
   ```
   dotnet build src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
   ```

### Running the Server

To run the MCP server, execute the following command:
```
dotnet run --project src/SemanticModelMcpServer/SemanticModelMcpServer.csproj
```

### Docker

To build and run the Docker container:
```
docker build -t mcp-server .
docker run -p 8080:80 mcp-server
```

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.

## Acknowledgments

- Microsoft for providing the Fabric REST API.
- The open-source community for their contributions and support.