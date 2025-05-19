using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer.Diagnostics
{    public class ToolSurfacingDiagnostics
    {        public static Task TestToolsRegisteringWithMcp()
        {
            Console.WriteLine("====== Testing Tool Registration with MCP ======");
            
            // Set up a cancellation token source to prevent hanging
            var timeoutCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
            var hasTimedOut = false;
            
            // Register timeout callback
            timeoutCts.Token.Register(() => 
            {
                hasTimedOut = true;
                Console.WriteLine("WARNING: Tool surfacing diagnostic timed out after 10 seconds.");
            });
            
            // List of expected tools
            var expectedTools = new Dictionary<string, Type>
            {
                { "createSemanticModel", typeof(CreateSemanticModelTool) },
                { "updateSemanticModel", typeof(UpdateSemanticModelTool) },
                { "deploySemanticModel", typeof(DeploymentTool) },
                { "refreshSemanticModel", typeof(RefreshTool) },  // added expected refresh tool
                { "validateTmdl", typeof(ValidateTmdlTool) },
                // Add other expected tools here
            };
            Console.WriteLine($"Expected number of tools: {expectedTools.Count}");
            
            try
            {
                // Get all registered tools through reflection
                var registeredTools = new Dictionary<string, Type>();
                
                foreach (var toolType in new[] { 
                    typeof(CreateSemanticModelTool),
                    typeof(UpdateSemanticModelTool),
                    typeof(RefreshTool),
                    typeof(DeploymentTool),
                    typeof(ValidateTmdlTool)
                })
                {                foreach (var method in toolType.GetMethods())
                    {
                        var attr = method.GetCustomAttribute<ModelContextProtocol.McpServerToolAttribute>();
                        if (attr != null)
                        {
                            registeredTools[attr.Name] = toolType;
                            Console.WriteLine($"Found tool {attr.Name} in {toolType.Name}.{method.Name}");
                        }
                    }
                }
                
                Console.WriteLine($"Found {registeredTools.Count} registered tools");
                
                // Compare expected vs. registered
                foreach (var expectedTool in expectedTools)
                {
                    if (registeredTools.TryGetValue(expectedTool.Key, out var registeredType))
                    {
                        Console.WriteLine($"Tool {expectedTool.Key} registered correctly as {registeredType.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Tool {expectedTool.Key} not found in registered tools");
                    }
                }
                
                // Check for extra tools
                foreach (var registeredTool in registeredTools)
                {
                    if (!expectedTools.ContainsKey(registeredTool.Key))
                    {
                        Console.WriteLine($"WARN: Unexpected tool {registeredTool.Key} found as {registeredTool.Value.Name}");
                    }
                }
                
                // Check tool initialization and dependencies
                Console.WriteLine("\nChecking tool dependencies and initialization:");
                foreach (var toolType in new[] { 
                    typeof(CreateSemanticModelTool),
                    typeof(UpdateSemanticModelTool),
                    typeof(RefreshTool),
                    typeof(DeploymentTool),
                    typeof(ValidateTmdlTool)
                })
                {
                    Console.WriteLine($"Tool: {toolType.Name}");
                    
                    // Check for constructor parameters/dependencies
                    var ctor = toolType.GetConstructors().FirstOrDefault();
                    if (ctor != null)
                    {
                        var parameters = ctor.GetParameters();
                        Console.WriteLine($"  Dependencies ({parameters.Length}):");
                        foreach (var param in parameters)
                        {
                            Console.WriteLine($"    {param.ParameterType.Name} {param.Name}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  ERROR: No constructor found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR during tool surfacing diagnosis: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
              Console.WriteLine("=================================================");
            
            // Make sure we don't hang if the operation takes too long
            if (hasTimedOut)
            {
                Console.WriteLine("WARNING: Tool surfacing diagnostic timed out after 10 seconds.");
            }
            
            // Return a completed task
            return Task.CompletedTask;
        }
    }
}
