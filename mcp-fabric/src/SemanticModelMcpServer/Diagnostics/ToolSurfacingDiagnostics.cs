using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Tools;
using SemanticModelMcpServer.Diagnostics;

namespace SemanticModelMcpServer.Diagnostics
{    public class ToolSurfacingDiagnostics
    {        public static Task TestToolsRegisteringWithMcp()
        {
            ConsoleHelper.Log("====== Testing Tool Registration with MCP ======");
            
            // Set up a cancellation token source to prevent hanging
            var timeoutCts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
            var hasTimedOut = false;
            
            // Register timeout callback
            timeoutCts.Token.Register(() => 
            {
                hasTimedOut = true;
                ConsoleHelper.Log("WARNING: Tool surfacing diagnostic timed out after 10 seconds.");
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
            ConsoleHelper.Log($"Expected number of tools: {expectedTools.Count}");
            
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
                            ConsoleHelper.Log($"Found tool {attr.Name} in {toolType.Name}.{method.Name}");
                        }
                    }
                }
                
                ConsoleHelper.Log($"Found {registeredTools.Count} registered tools");
                
                // Compare expected vs. registered
                foreach (var expectedTool in expectedTools)
                {
                    if (registeredTools.TryGetValue(expectedTool.Key, out var registeredType))
                    {
                        ConsoleHelper.Log($"Tool {expectedTool.Key} registered correctly as {registeredType.Name}");
                    }
                    else
                    {
                        ConsoleHelper.Log($"ERROR: Tool {expectedTool.Key} not found in registered tools");
                    }
                }
                
                // Check for extra tools
                foreach (var registeredTool in registeredTools)
                {
                    if (!expectedTools.ContainsKey(registeredTool.Key))
                    {
                        ConsoleHelper.Log($"WARN: Unexpected tool {registeredTool.Key} found as {registeredTool.Value.Name}");
                    }
                }
                
                // Check tool initialization and dependencies
                ConsoleHelper.Log("\nChecking tool dependencies and initialization:");
                foreach (var toolType in new[] { 
                    typeof(CreateSemanticModelTool),
                    typeof(UpdateSemanticModelTool),
                    typeof(RefreshTool),
                    typeof(DeploymentTool),
                    typeof(ValidateTmdlTool)
                })
                {
                    ConsoleHelper.Log($"Tool: {toolType.Name}");
                    
                    // Check for constructor parameters/dependencies
                    var ctor = toolType.GetConstructors().FirstOrDefault();
                    if (ctor != null)
                    {
                        var parameters = ctor.GetParameters();
                        ConsoleHelper.Log($"  Dependencies ({parameters.Length}):");
                        foreach (var param in parameters)
                        {
                            ConsoleHelper.Log($"    {param.ParameterType.Name} {param.Name}");
                        }
                    }
                    else
                    {
                        ConsoleHelper.Log("  ERROR: No constructor found");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.Log($"ERROR during tool surfacing diagnosis: {ex.Message}");
                ConsoleHelper.Log(ex.StackTrace);
            }
              ConsoleHelper.Log("=================================================");
            
            // Make sure we don't hang if the operation takes too long
            if (hasTimedOut)
            {
                ConsoleHelper.Log("WARNING: Tool surfacing diagnostic timed out after 10 seconds.");
            }
            
            // Return a completed task
            return Task.CompletedTask;
        }
    }
}
