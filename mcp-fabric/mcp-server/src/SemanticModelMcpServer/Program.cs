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
                .ConfigureServices(services =>
                {
                    // Add MCP server
                    services.AddMcpServer(server =>
                    {
                        // Register tools directly instead of using AddToolsFromAssembly
                        services.AddTransient<CreateSemanticModelTool>();
                        services.AddTransient<UpdateSemanticModelTool>();
                        services.AddTransient<RefreshTool>();
                        services.AddTransient<DeploymentTool>();
                        services.AddTransient<ValidateTmdlTool>();
                    });
                    
                    services.AddHttpClient<FabricClient>();
                    services.AddSingleton<IFabricClient, FabricClient>();
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