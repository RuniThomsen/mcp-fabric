using System;
using System.IO;
using System.Text.Json;

namespace SemanticModelMcpServer.Diagnostics
{
    public partial class McpServerDiagnostics
    {
        /// <summary>
        /// Validates that the mcp.json configuration file exists and contains valid JSON with required fields.
        /// </summary>
        /// <returns>True if the configuration is valid, false otherwise.</returns>
        public static bool ValidateMcpJsonConfiguration()
        {
            ConsoleHelper.Log("====== MCP Configuration Validation ======");
            
            try
            {
                // Check for mcp.json in the current directory
                var configPath = "mcp.json";
                
                // Check if the file exists
                if (!File.Exists(configPath))
                {
                    ConsoleHelper.Log($"ERROR: Configuration file '{configPath}' not found.");
                    return false;
                }
                
                ConsoleHelper.Log($"Found configuration file: {configPath}");
                
                // Read the file content
                string configJson;
                try
                {
                    configJson = File.ReadAllText(configPath);
                    ConsoleHelper.Log("Successfully read the configuration file.");
                }
                catch (Exception ex)
                {
                    ConsoleHelper.Log($"ERROR: Failed to read configuration file: {ex.Message}");
                    return false;
                }                // Parse the JSON to validate syntax
                JsonDocument document;
                try
                {
                    document = JsonDocument.Parse(configJson);
                    ConsoleHelper.Log("Configuration file contains valid JSON syntax.");
                }
                catch (JsonException ex)
                {
                    ConsoleHelper.Log($"ERROR: Invalid JSON syntax in configuration file: {ex.Message}");
                    return false;
                }
                
                // Check for required fields in the configuration
                var root = document.RootElement;
                
                // Check for servers section
                if (!root.TryGetProperty("servers", out var serversElement))
                {
                    ConsoleHelper.Log("ERROR: Required 'servers' section is missing from configuration.");
                    return false;
                }
                
                // Validate servers section structure
                if (serversElement.ValueKind != JsonValueKind.Object)
                {
                    ConsoleHelper.Log("ERROR: 'servers' section must be an object.");
                    return false;
                }
                
                // Check for at least one server definition
                bool hasServers = false;
                foreach (var serverProperty in serversElement.EnumerateObject())
                {
                    hasServers = true;
                    var serverName = serverProperty.Name;
                    var serverConfig = serverProperty.Value;
                    
                    // Check that each server has command and args
                    if (!serverConfig.TryGetProperty("command", out _))
                    {
                        ConsoleHelper.Log($"ERROR: Server '{serverName}' is missing required 'command' property.");
                        return false;
                    }
                    
                    if (!serverConfig.TryGetProperty("args", out var argsElement) || 
                        argsElement.ValueKind != JsonValueKind.Array)
                    {
                        ConsoleHelper.Log($"ERROR: Server '{serverName}' is missing required 'args' array property.");
                        return false;
                    }
                }
                
                if (!hasServers)
                {
                    ConsoleHelper.Log("ERROR: No server definitions found in 'servers' section.");
                    return false;
                }
                
                // Check for fabric API URL
                if (!root.TryGetProperty("fabricApiUrl", out _))
                {
                    ConsoleHelper.Log("WARNING: 'fabricApiUrl' property is missing from configuration.");
                    // Not a critical error, just a warning
                }
                
                // Check for authentication method (if needed)
                if (!root.TryGetProperty("authMethod", out _))
                {
                    ConsoleHelper.Log("WARNING: 'authMethod' property is missing from configuration.");
                    // Not a critical error, just a warning
                }
                  ConsoleHelper.Log("Configuration validation successful.");
                ConsoleHelper.Log("=========================================");
                return true;
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log($"ERROR: Unexpected error during configuration validation: {ex.Message}");
                ConsoleHelper.Log(ex.StackTrace);
                return false;
            }
        }
    }
}
