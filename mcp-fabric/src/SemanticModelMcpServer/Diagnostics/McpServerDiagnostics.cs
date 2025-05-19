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

namespace SemanticModelMcpServer.Diagnostics
{
    public class McpServerDiagnostics
    {
        public static void VerifyMcpServerConfiguration()
        {
            Console.WriteLine("====== MCP Server Configuration Diagnostic Tool ======");
            Console.WriteLine("Checking ModelContextProtocol configuration in assembly:");

            try
            {
                // Discover all loaded ModelContextProtocol assemblies
                var mcpAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => a.GetName().Name.StartsWith("ModelContextProtocol"))
                    .ToList();
                Console.WriteLine("Loaded ModelContextProtocol assemblies:");
                foreach (var asm in mcpAssemblies)
                {
                    Console.WriteLine($"  - {asm.GetName().Name}");
                }
                if (!mcpAssemblies.Any())
                {
                    Console.WriteLine("ERROR: No ModelContextProtocol assemblies loaded");
                }
                // Aggregate all types from ModelContextProtocol assemblies for diagnostics
                var mcpTypes = mcpAssemblies
                    .SelectMany(a => a.GetTypes())
                    .ToList();
                Console.WriteLine($"Total types loaded from ModelContextProtocol assemblies: {mcpTypes.Count}");

                // Identify the server builder type by name
                var mcpServerBuilderType = mcpTypes.FirstOrDefault(t => t.Name.IndexOf("McpServerBuilder", StringComparison.OrdinalIgnoreCase) >= 0
                    || t.Name.IndexOf("ServerBuilder", StringComparison.OrdinalIgnoreCase) >= 0);
                 if (mcpServerBuilderType != null)
                 {
                    Console.WriteLine($"Found server builder type: {mcpServerBuilderType.FullName}");
                    
                    // Check for WithStdioServerTransport method
                    var withStdioMethods = mcpServerBuilderType.GetMethods()
                        .Where(m => string.Equals(m.Name, "WithStdioServerTransport", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    
                    Console.WriteLine($"Found {withStdioMethods.Count} WithStdioServerTransport methods");
                    foreach (var method in withStdioMethods)
                    {
                        var parameters = method.GetParameters();
                        Console.WriteLine($"  Method with {parameters.Length} parameters");
                        foreach (var param in parameters)
                        {
                            Console.WriteLine($"    Parameter: {param.Name} of type {param.ParameterType.Name}");
                        }
                    }
                 }
                 else
                 {
                    Console.WriteLine("ERROR: Server builder type not found in any loaded ModelContextProtocol assembly");
                 }
                 
                // Check for static extension methods named WithStdioServerTransport across assemblies
                var extensionMethods = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .Where(m => string.Equals(m.Name, "WithStdioServerTransport", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                Console.WriteLine($"Found {extensionMethods.Count} static WithStdioServerTransport methods for extension");
                
                // Check for AddMcpServer extension method on IServiceCollection
                var addMcpServerMethod = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => string.Equals(m.Name, "AddMcpServer", StringComparison.OrdinalIgnoreCase));
                if (addMcpServerMethod != null)
                {
                    Console.WriteLine($"Found AddMcpServer method: {addMcpServerMethod.DeclaringType.FullName}.{addMcpServerMethod.Name} with {addMcpServerMethod.GetParameters().Length} parameters");
                }
                else
                {
                    Console.WriteLine("ERROR: AddMcpServer extension method not found on any ModelContextProtocol assembly");
                }

                // Check for WithToolsFromAssembly extension
                var withToolsMethod = mcpTypes
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    .FirstOrDefault(m => string.Equals(m.Name, "WithToolsFromAssembly", StringComparison.OrdinalIgnoreCase));
                if (withToolsMethod != null)
                {
                    Console.WriteLine($"Found WithToolsFromAssembly method: {withToolsMethod.DeclaringType?.FullName}.{withToolsMethod.Name}");
                }
                else
                {
                    Console.WriteLine("ERROR: WithToolsFromAssembly extension not found on any ModelContextProtocol assembly");
                }
                
                // Check tools capability configuration
                var toolsCapabilityType = typeof(ToolsCapability);
                Console.WriteLine($"ToolsCapability type: {toolsCapabilityType.FullName}");
                
                var toolsCapabilityProps = toolsCapabilityType.GetProperties();
                Console.WriteLine($"ToolsCapability has {toolsCapabilityProps.Length} properties");
                foreach (var prop in toolsCapabilityProps)
                {
                    Console.WriteLine($"  Property: {prop.Name} of type {prop.PropertyType.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during MCP server configuration check: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
              Console.WriteLine("=================================================");
        }
    }
}
