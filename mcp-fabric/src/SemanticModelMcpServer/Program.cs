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
                    // Add MCP server and register all tools from this assembly
                    services.AddMcpServer()
                        .WithStdioServerTransport() // Use StdioTransport for command-line tools
                        .WithToolsFromAssembly(Assembly.GetExecutingAssembly());

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