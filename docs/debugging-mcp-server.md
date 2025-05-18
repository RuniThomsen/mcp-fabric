# Semantic Model MCP Server Debugging Guide

## Project Context
This document contains debugging notes and lessons learned while troubleshooting the C# Semantic Model MCP Server. It serves as a knowledge base for common issues and their solutions to prevent recurrence.

## Date / Author
Initial creation: May 18, 2025 / GitHub Copilot

## Lessons Learned

### May 18, 2025
- Fixed port binding race by checking port availability before dotnet run
- Added proper exception handling for configuration file loading to prevent silent failures
- Implemented token masking in logs to prevent sensitive information exposure
- Resolved Docker container networking issues by explicitly setting host network mode
- Fixed MCP protocol handling by ensuring proper content-type headers in responses
- Improved configuration validation to catch malformed JSON in mcp.json before startup
