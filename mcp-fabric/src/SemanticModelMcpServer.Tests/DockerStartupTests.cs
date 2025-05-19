using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SemanticModelMcpServer.Tests
{
    public class DockerStartupTests
    {        [Fact]
        public async Task Docker_WithInvalidMcpJson_ReturnsNonZeroExitCode()
        {
            // Create a temporary directory for test files
            string tempDir = Path.Combine(Path.GetTempPath(), $"mcp-docker-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create an invalid mcp.json file (missing required fields)
                string invalidMcpJsonPath = Path.Combine(tempDir, "invalid-mcp.json");
                File.WriteAllText(invalidMcpJsonPath, @"{""servers"": {}}"); // Missing required server definitions

                // Build the Docker run command
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"run --pull=never -i --rm -v \"{invalidMcpJsonPath}:/app/mcp.json\" mcp-server:latest --validate-config-only",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the Docker process
                var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Docker process.");

                // Use CancellationToken to enforce a timeout
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                // Read stdout and stderr
                var stdoutTask = ReadProcessOutputAsync(process.StandardOutput, cts.Token);
                var stderrTask = ReadProcessOutputAsync(process.StandardError, cts.Token);
                
                // Wait for the process to exit or timeout
                var processExitTask = Task.Run(() => process.WaitForExit());
                var completedTask = await Task.WhenAny(processExitTask, Task.Delay(TimeSpan.FromSeconds(30)));
                
                if (completedTask != processExitTask)
                {
                    // Timeout occurred
                    process.Kill();
                    throw new TimeoutException("Docker process did not exit within the timeout period.");
                }

                // Process exited in time
                string stdout = await stdoutTask;
                string stderr = await stderrTask;

                // Assert that the process exited with a non-zero exit code
                Assert.NotEqual(0, process.ExitCode);
                
                // Assert that the error output contains validation error information
                Assert.Contains("ERROR", stderr);

                // Verify that the output contains specific validation messages
                Assert.Contains("No server definitions found in 'servers' section", stderr);
                
                // Output should indicate we're using the validation mode we added
                Assert.Contains("Validating MCP server configuration", stderr);
                Assert.Contains("MCP configuration validation failed", stderr);
            }
            finally
            {
                // Clean up the temp directory
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: Failed to clean up temp directory: {ex.Message}");
                }
            }
        }        [Fact]
        public async Task Docker_WithMalformedJsonMcpFile_ReturnsNonZeroExitCode()
        {
            // Create a temporary directory for test files
            string tempDir = Path.Combine(Path.GetTempPath(), $"mcp-docker-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create a malformed mcp.json file (invalid JSON syntax)
                string malformedMcpJsonPath = Path.Combine(tempDir, "malformed-mcp.json");
                File.WriteAllText(malformedMcpJsonPath, @"{""servers"": {,}}"); // Invalid JSON syntax

                // Build the Docker run command
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"run --pull=never -i --rm -v \"{malformedMcpJsonPath}:/app/mcp.json\" mcp-server:latest --validate-config-only",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the Docker process
                var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Docker process.");

                // Use CancellationToken to enforce a timeout
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                // Read stdout and stderr
                var stdoutTask = ReadProcessOutputAsync(process.StandardOutput, cts.Token);
                var stderrTask = ReadProcessOutputAsync(process.StandardError, cts.Token);
                
                // Wait for the process to exit or timeout
                var processExitTask = Task.Run(() => process.WaitForExit());
                var completedTask = await Task.WhenAny(processExitTask, Task.Delay(TimeSpan.FromSeconds(30)));
                
                if (completedTask != processExitTask)
                {
                    // Timeout occurred
                    process.Kill();
                    throw new TimeoutException("Docker process did not exit within the timeout period.");
                }

                // Process exited in time
                string stdout = await stdoutTask;
                string stderr = await stderrTask;

                // Assert that the process exited with a non-zero exit code
                Assert.NotEqual(0, process.ExitCode);
                
                // Assert that the error output contains JSON parsing error information
                Assert.Contains("ERROR", stderr);
                
                // Verify that the error mentions JSON parsing or invalid syntax
                Assert.Contains("Invalid JSON syntax", stderr);
                
                // Output should indicate we're using the validation mode we added
                Assert.Contains("Validating MCP server configuration", stderr);
                Assert.Contains("MCP configuration validation failed", stderr);
            }
            finally
            {
                // Clean up the temp directory
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: Failed to clean up temp directory: {ex.Message}");
                }
            }
        }[Fact]
        public async Task Docker_WithValidMcpJson_ReturnsZeroExitCode()
        {
            // Create a temporary directory for test files
            string tempDir = Path.Combine(Path.GetTempPath(), $"mcp-docker-test-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create a valid mcp.json file with all required fields
                string validMcpJsonPath = Path.Combine(tempDir, "valid-mcp.json");
                string validJson = @"{
                    ""servers"": {
                        ""testServer"": {
                            ""command"": ""echo"",
                            ""args"": [""test""],
                            ""env"": {}
                        }
                    },
                    ""fabricApiUrl"": ""https://api.fabric.microsoft.com"",
                    ""authMethod"": ""ManagedIdentity""
                }";
                File.WriteAllText(validMcpJsonPath, validJson);

                // Build the Docker run command
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = $"run --pull=never -i --rm -v \"{validMcpJsonPath}:/app/mcp.json\" mcp-server:latest --validate-config-only",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the Docker process
                var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Docker process.");

                // Use CancellationToken to enforce a timeout
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                // Read stdout and stderr
                var stdoutTask = ReadProcessOutputAsync(process.StandardOutput, cts.Token);
                var stderrTask = ReadProcessOutputAsync(process.StandardError, cts.Token);
                
                // Wait for the process to exit or timeout
                var processExitTask = Task.Run(() => process.WaitForExit());
                var completedTask = await Task.WhenAny(processExitTask, Task.Delay(TimeSpan.FromSeconds(30)));
                
                if (completedTask != processExitTask)
                {
                    // Timeout occurred
                    process.Kill();
                    throw new TimeoutException("Docker process did not exit within the timeout period.");
                }

                // Process exited in time
                string stdout = await stdoutTask;
                string stderr = await stderrTask;

                // Assert that the process exited with a zero exit code
                Assert.Equal(0, process.ExitCode);
                
                // Assert that the output contains success messages
                Assert.Contains("Configuration validation successful", stderr);
                Assert.Contains("MCP configuration validation succeeded", stderr);
            }
            finally
            {
                // Clean up the temp directory
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Warning: Failed to clean up temp directory: {ex.Message}");
                }
            }
        }        private static async Task<string> ReadProcessOutputAsync(StreamReader reader, CancellationToken cancellationToken = default)
        {
            var sb = new StringBuilder();
            
            // Using the overload that works with .NET Core/.NET 5+
            char[] buffer = new char[4096];
            int bytesRead;

            // ReadAsync overload compatible with .NET Core/.NET 5+
            while ((bytesRead = await reader.ReadAsync(buffer, cancellationToken)) > 0)
            {
                sb.Append(buffer, 0, bytesRead);
            }

            return sb.ToString();
        }
    }
}
