using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SemanticModelMcpServer.Tools;
using SemanticModelMcpServer.Diagnostics;

namespace SemanticModelMcpServer.Diagnostics
{
    public partial class McpServerDiagnostics
    {
        public static void VerifyMcpServerConfiguration()
        {
            ConsoleHelper.Log("====== MCP Server Configuration Diagnostic Tool ======");
            ConsoleHelper.Log("Checking ModelContextProtocol configuration in assembly:");

            try
            {
                // Discover all loaded ModelContextProtocol assemblies
                var mcpAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name.StartsWith("ModelContextProtocol"))
                    .ToList();
                ConsoleHelper.Log("Loaded ModelContextProtocol assemblies:");
                foreach (var asm in mcpAssemblies)
                {
                    ConsoleHelper.Log($"  - {asm.GetName().Name}");
                }
                if (!mcpAssemblies.Any())
                {
                    ConsoleHelper.Log("ERROR: No ModelContextProtocol assemblies loaded");
                }
                // Aggregate all types from ModelContextProtocol assemblies for diagnostics
                var mcpTypes = mcpAssemblies
                    .SelectMany(a => a.GetTypes())
                    .ToList();
                ConsoleHelper.Log($"Total types loaded from ModelContextProtocol assemblies: {mcpTypes.Count}");

                // Identify the server builder type by name
                var mcpServerBuilderType = mcpTypes.FirstOrDefault(t => t.Name.IndexOf("McpServerBuilder", StringComparison.OrdinalIgnoreCase) >= 0
                    || t.Name.IndexOf("ServerBuilder", StringComparison.OrdinalIgnoreCase) >= 0);
                 if (mcpServerBuilderType != null)
                 {
                    ConsoleHelper.Log($"Found server builder type: {mcpServerBuilderType.FullName}");
                    
                    // Check for WithStdioServerTransport method
                    var withStdioMethods = mcpServerBuilderType.GetMethods()
                        .Where(m => string.Equals(m.Name, "WithStdioServerTransport", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    
                    ConsoleHelper.Log($"Found {withStdioMethods.Count} WithStdioServerTransport methods");
                    foreach (var method in withStdioMethods)
                    {
                        var parameters = method.GetParameters();
                        ConsoleHelper.Log($"  Method with {parameters.Length} parameters");
                        foreach (var param in parameters)
                        {
                            ConsoleHelper.Log($"    Parameter: {param.Name} of type {param.ParameterType.Name}");
                        }
                    }
                 }
                 else
                 {
                    ConsoleHelper.Log("ERROR: Server builder type not found in any loaded ModelContextProtocol assembly");
                 }
                 
                // Check for static extension methods named WithStdioServerTransport across assemblies
                var extensionMethods = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(m => string.Equals(m.Name, "WithStdioServerTransport", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                ConsoleHelper.Log($"Found {extensionMethods.Count} static WithStdioServerTransport methods for extension");
                
                // Check for AddMcpServer extension method on IServiceCollection
                var addMcpServerMethod = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => string.Equals(m.Name, "AddMcpServer", StringComparison.OrdinalIgnoreCase));
                if (addMcpServerMethod != null)
                {
                    ConsoleHelper.Log($"Found AddMcpServer method: {addMcpServerMethod.DeclaringType.FullName}.{addMcpServerMethod.Name} with {addMcpServerMethod.GetParameters().Length} parameters");
                }
                else
                {
                    ConsoleHelper.Log("ERROR: AddMcpServer extension method not found on any ModelContextProtocol assembly");
                }

                // Check for WithToolsFromAssembly extension
                var withToolsMethod = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => string.Equals(m.Name, "WithToolsFromAssembly", StringComparison.OrdinalIgnoreCase));
                if (withToolsMethod != null)
                {
                    ConsoleHelper.Log($"Found WithToolsFromAssembly method: {withToolsMethod.DeclaringType?.FullName}.{withToolsMethod.Name}");
                }
                else
                {
                    ConsoleHelper.Log("ERROR: WithToolsFromAssembly extension not found on any ModelContextProtocol assembly");
                }
                
                // Check tools capability configuration
                var toolsCapabilityType = typeof(ToolsCapability);
                ConsoleHelper.Log($"ToolsCapability type: {toolsCapabilityType.FullName}");
                
                var toolsCapabilityProps = toolsCapabilityType.GetProperties();
                ConsoleHelper.Log($"ToolsCapability has {toolsCapabilityProps.Length} properties");
                foreach (var prop in toolsCapabilityProps)
                {
                    ConsoleHelper.Log($"  Property: {prop.Name} of type {prop.PropertyType.Name}");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log($"ERROR during MCP server configuration check: {ex.Message}");
                ConsoleHelper.Log(ex.StackTrace);
            }
              ConsoleHelper.Log("=================================================");
        }
    }
}
