using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpTool("validateTmdl", "Validates TMDL files for correctness and compatibility.")]
    public class ValidateTmdlTool
    {
        private readonly IPbiToolsRunner _pbiToolsRunner;

        public ValidateTmdlTool(IPbiToolsRunner pbiToolsRunner)
        {
            _pbiToolsRunner = pbiToolsRunner;
        }

        // Best practice: Return a dedicated DTO for validation results, not internal types
        public async Task<Services.ValidationResult> ValidateAsync(Dictionary<string, string> tmdlFiles)
        {
            if (tmdlFiles == null)
                throw new ArgumentNullException(nameof(tmdlFiles));
            // Error handling: exceptions are surfaced to caller for agent to handle
            var result = await _pbiToolsRunner.ValidateAsync(tmdlFiles);
            return result;
        }
    }
}