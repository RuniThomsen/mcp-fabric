using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // DEBUG: Log tool discovery details
                    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
                    var logger = loggerFactory.CreateLogger("ToolDiscovery");
                    var toolAssembly = typeof(CreateSemanticModelTool).Assembly;
                    logger.LogInformation("Scanning assembly: {Assembly}", toolAssembly.FullName);
                    var toolTypes = toolAssembly.GetTypes();
                    foreach (var type in toolTypes)
                    {
                        if (type.Name.EndsWith("Tool") && !type.IsInterface && !type.IsAbstract)
                        {
                            var hasTypeAttr = type.GetCustomAttributes(typeof(ModelContextProtocol.McpServerToolTypeAttribute), false).Length > 0;
                            logger.LogInformation("Tool type: {Type} [McpServerToolType: {HasAttr}]", type.FullName, hasTypeAttr);
                            foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                            {
                                var toolAttr = method.GetCustomAttribute<ModelContextProtocol.McpServerToolAttribute>();
                                if (toolAttr != null)
                                {
                                    logger.LogInformation("  Tool method: {Method} [McpServerTool: {ToolName}]", method.Name, toolAttr.Name);
                                }
                            }
                        }
                    }

                    // Add MCP server and register all tools from this assembly
                    services.AddMcpServer(options =>
                    {
                        options.ServerInfo = new Implementation 
                        {
                            Name = "Semantic Model MCP Server",
                            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
                        };
                        options.Capabilities = new ServerCapabilities
                        {
                            Tools = new ToolsCapability { }
                        };
                    })
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly(typeof(CreateSemanticModelTool).Assembly); // Ensures tool discovery

                    // Explicitly register all tool classes for DI
                    services.AddTransient<CreateSemanticModelTool>();
                    services.AddTransient<UpdateSemanticModelTool>();
                    services.AddTransient<RefreshTool>();
                    services.AddTransient<DeploymentTool>();
                    services.AddTransient<ValidateTmdlTool>();

                    // Use IHttpClientFactory for FabricClient
                    services.AddHttpClient<IFabricClient, FabricClient>();
                    services.AddSingleton<IPbiToolsRunner, PbiToolsRunner>();
                    services.AddTransient<TabularEditorRunner>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .RunConsoleAsync();
        }
    }
}