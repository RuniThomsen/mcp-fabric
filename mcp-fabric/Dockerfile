# Dockerfile

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ ./ 
RUN dotnet publish SemanticModelMcpServer/SemanticModelMcpServer.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
LABEL org.opencontainers.image.source="https://github.com/<org>/<repo>"
# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
RUN useradd -M -s /bin/bash -u 10001 -g 0 appuser
WORKDIR /app
COPY --from=build /app/publish ./ 
USER 10001
ENTRYPOINT ["dotnet", "SemanticModelMcpServer.dll"]

# Healthcheck for container best practice
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl --fail http://localhost:8080/health || exit 1