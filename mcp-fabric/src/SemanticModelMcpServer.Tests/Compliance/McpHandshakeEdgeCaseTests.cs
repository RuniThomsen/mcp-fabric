using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    /// <summary>
    /// Integration tests for handling various edge cases in the Model Context Protocol (MCP) handshake process.
    /// These tests verify the server's behavior when receiving malformed, incomplete, or invalid protocol messages.
    /// </summary>
    public class McpHandshakeEdgeCaseTests
    {
        private const string Jsonrpc = "2.0";
        private string ServerExePath => Path.Combine(AppContext.BaseDirectory, "SemanticModelMcpServer.exe");

        /// <summary>
        /// Verifies that the server correctly handles an incomplete initialize message.
        /// </summary>
        [Fact]
        public async Task Initialize_IncompleteMessage_ShouldReturnProtocolError()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Send an incomplete initialize message (missing required fields)
            var incompleteMessage = new
            {
                jsonrpc = Jsonrpc,
                id = 1,
                method = "initialize",
                @params = new { } // Missing required fields like protocolVersion and tools
            };
            
            // Send the message and get the response
            var response = await SendJsonRpcRequestAndGetResponseAsync(process, incompleteMessage);
            
            // Verify that the server returns a protocol error
            Assert.NotNull(response);
            Assert.Null(response.result);
            Assert.NotNull(response.error);
            Assert.Equal("Invalid initialize params", response.error.message);
            Assert.Equal(-32602, response.error.code); // Invalid params error code
            
            await ShutdownServerProcessAsync(process);
        }

        /// <summary>
        /// Verifies that the server correctly handles an initialize message with an unsupported protocol version.
        /// </summary>
        [Fact]
        public async Task Initialize_UnsupportedProtocolVersion_ShouldReturnVersionError()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Send an initialize message with an unsupported protocol version
            var unsupportedVersionMessage = new
            {
                jsonrpc = Jsonrpc,
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "999.0.0", // Unsupported version
                    tools = new[] { "createSemanticModel" }
                }
            };
            
            // Send the message and get the response
            var response = await SendJsonRpcRequestAndGetResponseAsync(process, unsupportedVersionMessage);
            
            // Verify that the server returns a protocol version error
            Assert.NotNull(response);
            Assert.Null(response.result);
            Assert.NotNull(response.error);
            Assert.Contains("Unsupported protocol version", response.error.message);
            
            await ShutdownServerProcessAsync(process);
        }

        /// <summary>
        /// Verifies that the server correctly handles malformed JSON in a message.
        /// </summary>
        [Fact]
        public async Task HandleRequest_MalformedJson_ShouldReturnParseError()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Send malformed JSON
            string malformedJson = @"{""jsonrpc"":""2.0"", ""id"":1, ""method"":""listTools"", unclosed";
            
            // Send the message directly (not using the helper method which expects valid JSON)
            var contentBytes = Encoding.UTF8.GetBytes(malformedJson);
            var message = $"Content-Length: {contentBytes.Length}\r\n\r\n{malformedJson}";
            await process.StandardInput.WriteAsync(message);
            await process.StandardInput.FlushAsync();
            
            // Give the server time to process
            await Task.Delay(500);
            
            // Read the response
            var responseMessage = await ReadResponseAsync(process);
            dynamic responseObj = JsonSerializer.Deserialize<dynamic>(responseMessage);
            
            // Verify that the server returns a parse error
            Assert.NotNull(responseObj);
            Assert.NotNull(responseObj.GetProperty("error"));
            Assert.Equal(-32700, responseObj.GetProperty("error").GetProperty("code").GetInt32());
            Assert.Contains("Parse error", responseObj.GetProperty("error").GetProperty("message").GetString());
            
            await ShutdownServerProcessAsync(process);
        }

        /// <summary>
        /// Verifies that the server correctly handles a request with an unknown method.
        /// </summary>
        [Fact]
        public async Task HandleRequest_UnknownMethod_ShouldReturnMethodNotFoundError()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Send a request with an unknown method
            var unknownMethodMessage = new
            {
                jsonrpc = Jsonrpc,
                id = 1,
                method = "nonExistentMethod"
            };
            
            // Send the message and get the response
            var response = await SendJsonRpcRequestAndGetResponseAsync(process, unknownMethodMessage);
            
            // Verify that the server returns a method not found error
            Assert.NotNull(response);
            Assert.Null(response.result);
            Assert.NotNull(response.error);
            Assert.Equal(-32601, response.error.code); // Method not found error code
            Assert.Contains("Method not found", response.error.message);
            
            await ShutdownServerProcessAsync(process);
        }

        /// <summary>
        /// Verifies that the server correctly handles multiple simultaneous requests.
        /// </summary>
        [Fact]
        public async Task HandleRequest_MultipleConcurrentRequests_ShouldProcessAllCorrectly()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Create multiple requests
            var request1 = new { jsonrpc = Jsonrpc, id = 1, method = "listTools" };
            var request2 = new { jsonrpc = Jsonrpc, id = 2, method = "listTools" };
            var request3 = new { jsonrpc = Jsonrpc, id = 3, method = "listTools" };
            
            // Send all requests without waiting for responses
            var task1 = SendJsonRpcRequestAsync(process, request1);
            var task2 = SendJsonRpcRequestAsync(process, request2);
            var task3 = SendJsonRpcRequestAsync(process, request3);
            
            // Wait for all send operations to complete
            await Task.WhenAll(task1, task2, task3);
            
            // Now read the responses and verify that all were processed correctly
            var responses = new dynamic[3];
            for (int i = 0; i < 3; i++)
            {
                var responseMessage = await ReadResponseAsync(process);
                responses[i] = JsonSerializer.Deserialize<dynamic>(responseMessage);
                
                // Verify that the response has the expected structure
                Assert.NotNull(responses[i]);
                Assert.NotNull(responses[i].GetProperty("result"));
                Assert.True(responses[i].GetProperty("result").GetProperty("tools").GetArrayLength() > 0);
            }
            
            // Verify that the responses have the correct ids
            var ids = new[] 
            { 
                responses[0].GetProperty("id").GetInt32(),
                responses[1].GetProperty("id").GetInt32(),
                responses[2].GetProperty("id").GetInt32()
            };
            Array.Sort(ids);
            Assert.Equal(new[] { 1, 2, 3 }, ids);
            
            await ShutdownServerProcessAsync(process);
        }

        /// <summary>
        /// Verifies that the server handles dropped connections gracefully.
        /// </summary>
        [Fact]
        public async Task HandleConnection_DroppedConnection_ShouldTerminateCleanly()
        {
            // Setup a process to start the MCP server
            using var process = StartServerProcess();
            
            // Send an initialize message
            var initializeMessage = new
            {
                jsonrpc = Jsonrpc,
                id = 1,
                method = "initialize",
                @params = new
                {
                    protocolVersion = "1.0.0",
                    tools = new[] { "createSemanticModel" }
                }
            };
            
            // Send the initialize message
            await SendJsonRpcRequestAsync(process, initializeMessage);
            
            // Read the response
            await ReadResponseAsync(process);
            
            // Now abruptly close the standard input (simulating a dropped connection)
            process.StandardInput.Close();
            
            // Verify that the process exits cleanly within a reasonable timeframe
            var exited = process.WaitForExit(5000); // 5 second timeout
            
            // Assert that the process exited
            Assert.True(exited, "The process should exit cleanly when the connection is dropped");
        }

        #region Helper Methods

        private Process StartServerProcess()
        {
            var psi = new ProcessStartInfo
            {
                FileName = ServerExePath,
                Arguments = "--stdio",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            if (process == null)
                throw new InvalidOperationException("Failed to start Semantic Model MCP Server process.");

            // Give the server time to initialize
            Thread.Sleep(1000);

            return process;
        }

        private async Task<dynamic> SendJsonRpcRequestAndGetResponseAsync(Process process, object request)
        {
            await SendJsonRpcRequestAsync(process, request);
            var responseMessage = await ReadResponseAsync(process);
            return JsonSerializer.Deserialize<dynamic>(responseMessage);
        }

        private async Task SendJsonRpcRequestAsync(Process process, object request)
        {
            string json = JsonSerializer.Serialize(request);
            byte[] contentBytes = Encoding.UTF8.GetBytes(json);
            string message = $"Content-Length: {contentBytes.Length}\r\n\r\n{json}";
            
            await process.StandardInput.WriteAsync(message);
            await process.StandardInput.FlushAsync();
        }

        private async Task<string> ReadResponseAsync(Process process, int timeoutMilliseconds = 5000)
        {
            var cancellationTokenSource = new CancellationTokenSource(timeoutMilliseconds);
            var responseBuilder = new StringBuilder();
            var buffer = new char[1];
            bool headerComplete = false;
            int contentLength = 0;
            var headerBuilder = new StringBuilder();

            try
            {
                // First, read the header to get the content length
                while (!headerComplete && !cancellationTokenSource.IsCancellationRequested)
                {
                    if (await process.StandardOutput.ReadAsync(buffer, 0, 1) > 0)
                    {
                        headerBuilder.Append(buffer[0]);
                        
                        // Check if we've reached the end of the header
                        if (headerBuilder.ToString().EndsWith("\r\n\r\n"))
                        {
                            headerComplete = true;
                            string header = headerBuilder.ToString();
                            
                            // Extract the content length
                            var match = System.Text.RegularExpressions.Regex.Match(header, @"Content-Length: (\d+)");
                            if (match.Success)
                            {
                                contentLength = int.Parse(match.Groups[1].Value);
                            }
                            else
                            {
                                throw new InvalidOperationException("Failed to extract content length from header.");
                            }
                        }
                    }
                }

                // Now read the content
                var content = new char[contentLength];
                int bytesRead = 0;
                
                while (bytesRead < contentLength && !cancellationTokenSource.IsCancellationRequested)
                {
                    int read = await process.StandardOutput.ReadAsync(content, bytesRead, contentLength - bytesRead);
                    if (read > 0)
                    {
                        bytesRead += read;
                    }
                }
                
                return new string(content, 0, bytesRead);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Timeout waiting for response from the MCP server.");
            }
        }

        private async Task ShutdownServerProcessAsync(Process process)
        {
            try
            {
                // Send a proper exit command
                var exitCommand = new
                {
                    jsonrpc = Jsonrpc,
                    id = "test-exit",
                    method = "exit"
                };
                
                await SendJsonRpcRequestAsync(process, exitCommand);
                
                // Give it some time to process the exit command
                await Task.Delay(500);
                
                // Close the input stream
                process.StandardInput.Close();
                
                // Wait for the process to exit
                if (!process.WaitForExit(5000))
                {
                    // Force kill if it doesn't exit cleanly
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during server shutdown: {ex.Message}");
                
                if (!process.HasExited)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        // Ignore errors during forced termination
                    }
                }
            }
        }

        #endregion
    }
}
