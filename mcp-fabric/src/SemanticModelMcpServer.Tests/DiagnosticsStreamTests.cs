using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SemanticModelMcpServer;
using Xunit;

namespace SemanticModelMcpServer.Tests
{
    public class DiagnosticsStreamTests
    {        [Fact]
        public async Task DiagnosticMessages_AreWrittenToStandardErrorOnly()
        {
            // Locate the server executable in the publish output of the current project
            string projectDir = AppContext.BaseDirectory;
            string exePath = Path.Combine(projectDir, "SemanticModelMcpServer.exe");

            // Add a --diagnostics-only parameter to ensure the server exits after diagnostics
            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "--diagnostics-only", // New parameter to run only diagnostics phase
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = null;
            try
            {
                process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Semantic Model MCP Server process.");

                // Create tasks to read stdout and stderr asynchronously
                var stdoutTask = ReadProcessOutputAsync(process.StandardOutput);
                var stderrTask = ReadProcessOutputAsync(process.StandardError);

                // Implement two-phase shutdown with timeout
                // Phase 1: Send exit command via JSON-RPC
                var exitCommand = new
                {
                    jsonrpc = "2.0",
                    id = "test-exit",
                    method = "exit",
                    @params = new { }
                };
                string exitJson = System.Text.Json.JsonSerializer.Serialize(exitCommand);
                await process.StandardInput.WriteLineAsync(exitJson);
                await process.StandardInput.FlushAsync();
                
                // Give the server time to process the exit command
                await Task.Delay(1000);
                
                // Phase 2: Close standard input
                process.StandardInput.Close();

                // Wait for output capture to complete with timeout
                var outputCaptureTimeout = Task.Delay(TimeSpan.FromSeconds(10));
                
                // Wait for all output to be captured or timeout
                var completedTask = await Task.WhenAny(
                    Task.WhenAll(stdoutTask, stderrTask),
                    outputCaptureTimeout
                );
                
                if (completedTask == outputCaptureTimeout)
                {
                    Debug.WriteLine("WARNING: Output capture timed out after 10 seconds");
                }

                // Get the captured output
                string stdout = await stdoutTask;
                string stderr = await stderrTask;

                // Verify stdout does not contain diagnostic messages
                Assert.DoesNotContain("MCP Server Configuration Diagnostic Tool", stdout);
                Assert.DoesNotContain("MCP Tool Registration Diagnostic Tool", stdout);

                // Verify stderr contains diagnostic messages
                Assert.Contains("MCP Server Configuration Diagnostic Tool", stderr);
                Assert.Contains("MCP Tool Registration Diagnostic Tool", stderr);

                // Wait for process to exit gracefully
                bool exited = process.WaitForExit(5000); // Wait up to 5 seconds
                if (!exited)
                {
                    Debug.WriteLine("Process did not exit within 5 seconds, forcing termination");
                }
            }
            finally
            {
                // Ensure process is properly cleaned up
                if (process != null && !process.HasExited)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(2000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERROR: Failed to terminate process: {ex.Message}");
                    }
                }
                
                process?.Dispose();
            }
        }

        // Helper method to read process output asynchronously
        private static async Task<string> ReadProcessOutputAsync(StreamReader reader)
        {
            var output = new StringBuilder();
            var buffer = new char[4096];
            int bytesRead;

            while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                output.Append(buffer, 0, bytesRead);
            }

            return output.ToString();
        }
    }
}
