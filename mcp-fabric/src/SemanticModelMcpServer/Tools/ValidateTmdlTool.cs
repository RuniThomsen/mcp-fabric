using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpServerToolType]
    public class ValidateTmdlTool
    {
        private readonly IPbiToolsRunner _pbiToolsRunner;

        public ValidateTmdlTool(IPbiToolsRunner pbiToolsRunner)
        {
            _pbiToolsRunner = pbiToolsRunner;
        }

        [McpServerTool("validateTmdl")]
        [Description("Validates TMDL files for syntax and semantic errors.")]
        public async Task<Services.ValidationResult> ValidateAsync(Dictionary<string, string> tmdlFiles)
        {
            if (tmdlFiles == null)
                throw new ArgumentNullException(nameof(tmdlFiles));
            var result = await _pbiToolsRunner.ValidateAsync(tmdlFiles);
            return result;
        }
    }
}