using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using SemanticModelMcpServer.Diagnostics;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer
{
    public class Program
    {        
        // Command-line arguments
        private const string DiagnosticsOnlyArgument = "--diagnostics-only";
        private const string ValidateConfigOnlyArgument = "--validate-config-only";
        
        public static async Task Main(string[] args)
        {
            bool diagnosticsOnlyMode = args.Contains(DiagnosticsOnlyArgument);
            bool validateConfigOnlyMode = args.Contains(ValidateConfigOnlyArgument);
            
            // Redirect diagnostic output to stderr so JSON-RPC stays on stdout
            Console.Error.WriteLine("Semantic Model MCP Server starting up...");            // If in validate-config-only mode, only validate the mcp.json configuration
            if (validateConfigOnlyMode)
            {
                try
                {
                    Console.Error.WriteLine("Validating MCP server configuration...");
                    bool isValid = McpServerDiagnostics.ValidateMcpJsonConfiguration();
                    if (!isValid)
                    {
                        Console.Error.WriteLine("MCP configuration validation failed. See errors above.");
                        Environment.Exit(1);
                    }
                    Console.Error.WriteLine("MCP configuration validation succeeded.");
                    Environment.Exit(0); // Exit with success code
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error validating MCP configuration: {ex.Message}");
                    Environment.Exit(1);
                }
            }
            
            // Run tool registration diagnostics
            Console.Error.WriteLine("Performing tool registration diagnostics...");
            ToolRegistrationDiagnostics.VerifyToolRegistrations();
            
            // Run MCP server configuration diagnostics
            Console.Error.WriteLine("\nPerforming MCP server configuration diagnostics...");
            McpServerDiagnostics.VerifyMcpServerConfiguration();
            // Test tool registration with MCP
            Console.Error.WriteLine("\nTesting tool registration with MCP...");
            ToolSurfacingDiagnostics.TestToolsRegisteringWithMcp().GetAwaiter().GetResult();
            
            // Run SDK compliance verification
            Console.Error.WriteLine("\nVerifying SDK compliance...");
            SdkComplianceVerification.VerifySdkCompliance();

            // If in diagnostics-only mode, exit after running diagnostics
            if (diagnosticsOnlyMode)
            {
                Console.Error.WriteLine("Diagnostics completed. Exiting in --diagnostics-only mode.");
                return; // Exit without starting the actual server
            }

            // Check if port 8080 is already in use
            try
            {
                var isPortInUse = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpListeners()
                    .Any(x => x.Port == 8080);
                
                if (isPortInUse)
                {
                    Console.Error.WriteLine("ERROR: Port 8080 is already in use. The server cannot start.");
                    Console.Error.WriteLine("Please ensure port 8080 is available before starting the server.");
                    Environment.Exit(1);
                }
                else
                {
                    Console.Error.WriteLine("Port 8080 is available for use.");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to check port availability: {ex.Message}");
                Console.Error.WriteLine("Proceeding with caution, but this may cause connection issues.");
            }

            // Validate mcp.json exists and is valid
            try
            {
                var configPath = "mcp.json";
                if (File.Exists(configPath))
                {
                    var configJson = File.ReadAllText(configPath);
                    System.Text.Json.JsonDocument.Parse(configJson);
                    Console.Error.WriteLine("Configuration file mcp.json is valid.");
                    
                    // Check for required fields in the configuration
                    var document = System.Text.Json.JsonDocument.Parse(configJson);
                    var root = document.RootElement;
                    
                    // Check for key fields like API endpoints
                    if (!root.TryGetProperty("fabricApiUrl", out _))
                    {
                        Console.Error.WriteLine("Warning: mcp.json is missing 'fabricApiUrl' property.");
                    }
                    
                    if (!root.TryGetProperty("authMethod", out _))
                    {
                        Console.Error.WriteLine("Warning: mcp.json is missing 'authMethod' property.");
                    }
                }
                else
                {
                    Console.Error.WriteLine($"ERROR: Configuration file {configPath} not found.");
                    Console.Error.WriteLine("Please create a valid mcp.json configuration file in the application root directory.");
                    Environment.Exit(1);
                }
            }
            catch (System.Text.Json.JsonException ex)
            {
                Console.Error.WriteLine($"ERROR: Invalid JSON in mcp.json: {ex.Message}");
                Console.Error.WriteLine("Please fix the JSON syntax errors in the configuration file.");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error validating mcp.json: {ex.Message}");
                Console.Error.WriteLine("Proceeding with caution, but this may cause operational issues.");
            }

            await Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>                {
                    // DEBUG: Log tool discovery details
                    var loggerFactory = LoggerFactory.Create(builder => 
                        builder.AddConsole(opts => {
                            // Force ALL console logs to go to stderr
                            opts.LogToStandardErrorThreshold = LogLevel.Trace;
                        })
                        .SetMinimumLevel(LogLevel.Debug)
                    );
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
                            
                            // Check and log any issues with tool methods
                            var hasToolMethod = false;
                            foreach (var method in type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
                            {
                                var toolAttr = method.GetCustomAttribute<ModelContextProtocol.McpServerToolAttribute>();
                                if (toolAttr != null)
                                {
                                    hasToolMethod = true;
                                    logger.LogInformation("  Tool method: {Method} [McpServerTool: {ToolName}]", method.Name, toolAttr.Name);
                                    
                                    // Validate method signature for MCP compatibility
                                    if (!method.ReturnType.IsGenericType || !method.ReturnType.GetGenericTypeDefinition().Equals(typeof(Task<>)))
                                    {
                                        logger.LogWarning("  Warning: Tool method {Method} does not return Task<T>, which may cause issues with MCP", method.Name);
                                    }
                                    
                                    var parameters = method.GetParameters();
                                    if (parameters.Length != 1)
                                    {
                                        logger.LogWarning("  Warning: Tool method {Method} has {Count} parameters, expected exactly 1", method.Name, parameters.Length);
                                    }
                                }
                            }
                            
                            if (hasTypeAttr && !hasToolMethod)
                            {
                                logger.LogWarning("  Warning: Tool type {Type} has [McpServerToolType] attribute but no methods with [McpServerTool] attribute", type.FullName);
                            }
                        }
                    }                    // Add MCP server and register all tools from this assembly
                    services.AddMcpServer(options =>
                    {
                        options.ServerInfo = new Implementation 
                        {
                            Name = "Semantic Model MCP Server",
                            Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0"
                        };
                    })
                    .WithStdioServerTransport()  // Using the standard version without options
                    .WithToolsFromAssembly(typeof(Program).Assembly);  // Register all tools from this assembly

                    // Register all services needed by tools
                    services.AddHttpClient<IFabricClient, FabricClient>();
                    services.AddSingleton<IPbiToolsRunner, PbiToolsRunner>();
                    services.AddTransient<TabularEditorRunner>();
                    services.AddHostedService<HealthProbeHostedService>(); // Add health probe service
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // <-- Add this line!
                    logging.AddConsole(options =>
                    {
                        options.LogToStandardErrorThreshold = LogLevel.Trace;
                    });
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .RunConsoleAsync();
        }
    }
}