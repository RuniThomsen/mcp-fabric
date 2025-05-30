using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Tools;
using SemanticModelMcpServer.Diagnostics;

namespace SemanticModelMcpServer.Diagnostics
{
    public class ToolRegistrationDiagnostics
    {
        public static void VerifyToolRegistrations()
        {
            ConsoleHelper.Log("====== MCP Tool Registration Diagnostic Tool ======");
            ConsoleHelper.Log("Checking tool registrations in assembly: " + typeof(CreateSemanticModelTool).Assembly.FullName);
            ConsoleHelper.Log("");

            var toolTypes = new[]
            {
                typeof(CreateSemanticModelTool),
                typeof(UpdateSemanticModelTool),
                typeof(RefreshTool),
                typeof(DeploymentTool),
                typeof(ValidateTmdlTool)
            };

            bool hasIssues = false;

            foreach (var type in toolTypes)
            {
                ConsoleHelper.Log($"Checking tool class: {type.Name}");
                
                // Check for McpServerToolTypeAttribute
                var hasTypeAttr = type.GetCustomAttributes(typeof(McpServerToolTypeAttribute), inherit: false).Any();
                ConsoleHelper.Log($"  Has [McpServerToolType] attribute: {hasTypeAttr}");
                
                if (!hasTypeAttr)
                {
                    ConsoleHelper.Log($"  ERROR: {type.Name} is missing the [McpServerToolType] attribute");
                    hasIssues = true;
                }
                
                // Check for methods with McpServerToolAttribute
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any())
                    .ToList();
                
                ConsoleHelper.Log($"  Methods with [McpServerTool] attribute: {methods.Count}");
                
                if (methods.Count == 0)
                {
                    ConsoleHelper.Log($"  ERROR: {type.Name} has no methods with [McpServerTool] attribute");
                    hasIssues = true;
                }
                
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<McpServerToolAttribute>();
                    ConsoleHelper.Log($"  - Method: {method.Name}, Tool name: {attr.Name}");
                    
                    // Check return type
                    if (!method.ReturnType.IsGenericType || !method.ReturnType.GetGenericTypeDefinition().Equals(typeof(Task<>)))
                    {
                        ConsoleHelper.Log($"    ERROR: Method {method.Name} does not return Task<T>");
                        hasIssues = true;
                    }
                    
                    // Check parameters
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        ConsoleHelper.Log($"    ERROR: Method {method.Name} has {parameters.Length} parameters, expected exactly 1");
                        hasIssues = true;
                    }
                    else
                    {
                        ConsoleHelper.Log($"    Parameter type: {parameters[0].ParameterType.Name}");
                    }
                }
                
                ConsoleHelper.Log("");
            }
            
            ConsoleHelper.Log("====== Diagnostic Summary ======");
            ConsoleHelper.Log($"Found {toolTypes.Length} tool classes");
            
            if (hasIssues)
            {
                ConsoleHelper.Log("Issues were found with tool registrations. See details above.");
            }
            else
            {
                ConsoleHelper.Log("All tools appear to be correctly registered.");
            }
            
            ConsoleHelper.Log("================================");
        }
    }
}
