FROM mcr.microsoft.com/devcontainers/dotnet:8.0

# Install PowerShell
RUN apt-get update \
    && apt-get install -y wget apt-transport-https software-properties-common \
    && wget -q "https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb" \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell \
    && rm packages-microsoft-prod.deb

# Install Power BI Tools (optional, uncomment if needed)
# RUN dotnet tool install --global PBI.Tools --version 1.*

# Configure environment for Power BI development
ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_ENVIRONMENT=Development

# Create a script to setup credentials (template - user fills in values later)
COPY .devcontainer/scripts/setup-credentials.sh /tmp/
RUN chmod +x /tmp/setup-credentials.sh
