using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer.Extensions
{
    public static class McpServerBuilderExtensions
    {
        // Custom extension method to register a single tool type
        public static IMcpServerBuilder WithExplicitTool<T>(this IMcpServerBuilder builder) where T : class
        {            Console.Error.WriteLine($"Explicitly registering tool of type {typeof(T).Name}");

            // Manually check that the type has the McpServerToolTypeAttribute
            var hasAttribute = typeof(T).GetCustomAttributes(typeof(ModelContextProtocol.Server.McpServerToolTypeAttribute), inherit: false).Length > 0;
            if (!hasAttribute)
            {
                Console.Error.WriteLine($"WARNING: Type {typeof(T).Name} does not have McpServerToolTypeAttribute");
            }

            // Find tool methods
            var toolMethods = 0;
            foreach (var method in typeof(T).GetMethods())
            {
                var attr = method.GetCustomAttribute<ModelContextProtocol.Server.McpServerToolAttribute>();
                if (attr != null)
                {
                    toolMethods++;
                    Console.Error.WriteLine($"Found tool method: {method.Name} with name '{attr.Name}'");
                }
            }

            if (toolMethods == 0)
            {
                Console.Error.WriteLine($"WARNING: Type {typeof(T).Name} has no methods with McpServerToolAttribute");
            }

            // Try to register manually if WithToolType is available
            try
            {
                var withToolTypeMethod = builder.GetType().GetMethod("WithToolType");
                if (withToolTypeMethod != null)
                {
                    Console.Error.WriteLine($"Calling WithToolType for {typeof(T).Name}");
                    withToolTypeMethod.MakeGenericMethod(typeof(T)).Invoke(builder, null);
                }
                else
                {
                    Console.Error.WriteLine("WARNING: WithToolType method not found on builder");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR registering tool: {ex.Message}");
            }

            return builder;
        }
    }
}
