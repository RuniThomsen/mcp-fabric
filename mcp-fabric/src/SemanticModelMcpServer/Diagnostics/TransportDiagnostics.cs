using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SemanticModelMcpServer.Diagnostics;

namespace SemanticModelMcpServer.Diagnostics
{
    public class TransportDiagnostics
    {
        private readonly ILogger<TransportDiagnostics> _logger;
        
        public TransportDiagnostics(ILogger<TransportDiagnostics> logger)
        {
            _logger = logger;
        }
        
        public async Task RunStdioTransportTest()
        {
            _logger.LogInformation("Testing standard IO transport...");
            
            // Write to console out
            ConsoleHelper.Log("MCP Transport Test: Writing to Console.Out");
            
            // Write to console error
            Console.Error.WriteLine("MCP Transport Test: Writing to Console.Error");
            
            // Test reading from standard input
            _logger.LogInformation("Testing standard input...");
            
            // Create a separate task to simulate input
            var simulateInputTask = Task.Run(() => 
            {
                Thread.Sleep(1000); // Wait a bit to make sure the reader is ready
                using (var writer = new StreamWriter(Console.OpenStandardInput(), Encoding.UTF8, 1024, leaveOpen: true))
                {
                    writer.WriteLine("{\"jsonrpc\":\"2.0\",\"id\":\"test-1\",\"method\":\"test\",\"params\":{}}");
                    writer.Flush();
                }
                _logger.LogInformation("Simulated input sent");
            });
            
            // Try to read from standard input
            using (var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: false, 1024, leaveOpen: true))
            {
                _logger.LogInformation("Waiting for input...");
                var line = await reader.ReadLineAsync();
                _logger.LogInformation("Received input: {Input}", line ?? "(null)");
            }
            
            await simulateInputTask;
            
            _logger.LogInformation("Standard IO transport test completed");
        }
    }
}
