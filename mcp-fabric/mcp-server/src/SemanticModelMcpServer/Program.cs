using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    // Add MCP server and auto-register all tools in this assembly
                    services.AddMcpServer(server =>
                    {
                        // Best practice: Use server.AddToolsFromAssembly(Assembly.GetExecutingAssembly()) if available in your MCP SDK
                        // Fallback: Register tools explicitly
                        // Best practice: Use server.AddToolsFromAssembly(Assembly.GetExecutingAssembly()) if available in your MCP SDK
                        // Fallback: Register tools explicitly
                        services.AddTransient<CreateSemanticModelTool>();
                        services.AddTransient<UpdateSemanticModelTool>();
                        services.AddTransient<RefreshTool>();
                        services.AddTransient<DeploymentTool>();
                        services.AddTransient<ValidateTmdlTool>();
                    });

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