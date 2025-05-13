using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SemanticModelMcpServer.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    
    public class PbiToolsRunner : IPbiToolsRunner
    {
        private readonly ILogger<PbiToolsRunner> _logger;
        
        public PbiToolsRunner(ILogger<PbiToolsRunner> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<string> RunPbiToolsCommandAsync(string command, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Error running command: {error}");
            }

            return output;
        }
        
        public async Task<ValidationResult> ValidateAsync(Dictionary<string, string> tmdlFiles)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                // Write TMDL files to temporary directory
                foreach (var (filePath, content) in tmdlFiles)
                {
                    var fullPath = Path.Combine(tempDir, filePath);
                    var directory = Path.GetDirectoryName(fullPath);
                    
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    
                    File.WriteAllText(fullPath, content);
                }
                
                // Run pbi-tools validate
                var result = new ValidationResult { IsValid = true };
                
                try
                {
                    var output = await RunPbiToolsCommandAsync("pbi-tools", $"validate \"{tempDir}\"");
                    result.IsValid = !output.Contains("error") && !output.Contains("failed");
                    
                    if (!result.IsValid)
                    {
                        // Parse errors from output
                        var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var line in lines)
                        {
                            if (line.Contains("error"))
                            {
                                result.Errors.Add(line.Trim());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Validation process error: {ex.Message}");
                }
                
                return result;
            }
            finally
            {
                // Clean up temp directory
                if (Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch (IOException ex)
                    {
                        _logger.LogWarning("Failed to delete temporary directory: {Message}", ex.Message);
                    }
                }
            }
        }
        
        public async Task<ValidationResult> ValidateTmdlAsync(string tmdlPath)
        {
            if (!Directory.Exists(tmdlPath))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { $"Directory not found: {tmdlPath}" }
                };
            }
            
            var result = new ValidationResult { IsValid = true };
            
            try
            {
                var output = await RunPbiToolsCommandAsync("pbi-tools", $"validate \"{tmdlPath}\"");
                result.IsValid = !output.Contains("error") && !output.Contains("failed");
                
                if (!result.IsValid)
                {
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.Contains("error"))
                        {
                            result.Errors.Add(line.Trim());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Validation process error: {ex.Message}");
            }
            
            return result;
        }
    }
}