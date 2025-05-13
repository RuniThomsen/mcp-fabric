# Dockerfile

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ ./ 
RUN dotnet publish SemanticModelMcpServer/SemanticModelMcpServer.csproj -c Release -o /app/publish -p:PublishSingleFile=true -p:PublishTrimmed=true --runtime linux-x64 --self-contained true

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
LABEL org.opencontainers.image.source="https://github.com/<org>/<repo>"
RUN adduser --system --uid 10001 appuser
WORKDIR /app
COPY --from=build /app/publish ./ 
USER 10001
ENTRYPOINT ["./SemanticModelMcpServer"]