using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer.Diagnostics
{
    public class ToolRegistrationDiagnostics
    {
        public static void VerifyToolRegistrations()
        {
            Console.WriteLine("====== MCP Tool Registration Diagnostic Tool ======");
            Console.WriteLine("Checking tool registrations in assembly: " + typeof(CreateSemanticModelTool).Assembly.FullName);
            Console.WriteLine();

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
                Console.WriteLine($"Checking tool class: {type.Name}");
                
                // Check for McpServerToolTypeAttribute
                var hasTypeAttr = type.GetCustomAttributes(typeof(McpServerToolTypeAttribute), inherit: false).Any();
                Console.WriteLine($"  Has [McpServerToolType] attribute: {hasTypeAttr}");
                
                if (!hasTypeAttr)
                {
                    Console.WriteLine($"  ERROR: {type.Name} is missing the [McpServerToolType] attribute");
                    hasIssues = true;
                }
                
                // Check for methods with McpServerToolAttribute
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any())
                    .ToList();
                
                Console.WriteLine($"  Methods with [McpServerTool] attribute: {methods.Count}");
                
                if (methods.Count == 0)
                {
                    Console.WriteLine($"  ERROR: {type.Name} has no methods with [McpServerTool] attribute");
                    hasIssues = true;
                }
                
                foreach (var method in methods)
                {
                    var attr = method.GetCustomAttribute<McpServerToolAttribute>();
                    Console.WriteLine($"  - Method: {method.Name}, Tool name: {attr.Name}");
                    
                    // Check return type
                    if (!method.ReturnType.IsGenericType || !method.ReturnType.GetGenericTypeDefinition().Equals(typeof(Task<>)))
                    {
                        Console.WriteLine($"    ERROR: Method {method.Name} does not return Task<T>");
                        hasIssues = true;
                    }
                    
                    // Check parameters
                    var parameters = method.GetParameters();
                    if (parameters.Length != 1)
                    {
                        Console.WriteLine($"    ERROR: Method {method.Name} has {parameters.Length} parameters, expected exactly 1");
                        hasIssues = true;
                    }
                    else
                    {
                        Console.WriteLine($"    Parameter type: {parameters[0].ParameterType.Name}");
                    }
                }
                
                Console.WriteLine();
            }
            
            Console.WriteLine("====== Diagnostic Summary ======");
            Console.WriteLine($"Found {toolTypes.Length} tool classes");
            
            if (hasIssues)
            {
                Console.WriteLine("Issues were found with tool registrations. See details above.");
            }
            else
            {
                Console.WriteLine("All tools appear to be correctly registered.");
            }
            
            Console.WriteLine("================================");
        }
    }
}
