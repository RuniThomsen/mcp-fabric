# Dockerfile

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ ./

# ARG for build target architecture (default to x64, but can be overridden)
ARG TARGETARCH=amd64

# Set the appropriate runtime based on the target architecture
RUN if [ "$TARGETARCH" = "arm64" ]; then \
        RUNTIME="linux-arm64"; \
    else \
        RUNTIME="linux-x64"; \
    fi && \
    dotnet publish SemanticModelMcpServer/SemanticModelMcpServer.csproj -c Release -o /app/publish -p:PublishSingleFile=true -p:PublishTrimmed=true --runtime $RUNTIME --self-contained true

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0
LABEL org.opencontainers.image.source="https://github.com/<org>/<repo>"

# Add support for ARM64 platforms
ARG TARGETARCH=amd64
LABEL architecture="${TARGETARCH}"

RUN adduser --system --uid 10001 appuser
WORKDIR /app
COPY --from=build /app/publish ./ 
USER 10001
ENTRYPOINT ["./SemanticModelMcpServer"]

# Healthcheck for container best practice
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 CMD curl --fail http://localhost:80/health || exit 1

# (Optional) Add SBOM generation in CI pipeline, e.g. with github/sbom-action
# See: https://github.com/github/sbom-action