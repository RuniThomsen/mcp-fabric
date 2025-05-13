using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Server;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer
{
    public class Program
    {
        private static int GetPortFromArgs(string[] args)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--port" && int.TryParse(args[i + 1], out int port))
                {
                    return port;
                }
            }
            return 5000; // Default port
        }

        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var mcpServer = services.AddMcpServer();

                    // Configure transport protocol based on command-line args
                    if (args.Contains("--http"))
                    {
                        var port = GetPortFromArgs(args);
                        Console.WriteLine($"Starting MCP server with HTTP transport on port {port}");
                        Console.WriteLine("Warning: HTTP transport is not fully implemented yet in this version");
                        // TODO: Uncomment when HTTP transport is available in the SDK
                        // mcpServer.WithHttpServerTransport(port);
                        
                        // For now, fall back to stdio transport
                        mcpServer.WithStdioServerTransport();
                    }
                    else
                    {
                        Console.WriteLine("Starting MCP server with stdio transport");
                        mcpServer.WithStdioServerTransport();
                    }

                    // Register all tools from this assembly
                    mcpServer.WithToolsFromAssembly(Assembly.GetExecutingAssembly());

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