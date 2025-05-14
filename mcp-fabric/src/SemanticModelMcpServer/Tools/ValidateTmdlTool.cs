using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpServerToolType]
    public class ValidateTmdlTool
    {
        private readonly IPbiToolsRunner _pbiToolsRunner;
        private readonly ILogger<ValidateTmdlTool> _logger;

        public ValidateTmdlTool(IPbiToolsRunner pbiToolsRunner, ILogger<ValidateTmdlTool> logger)
        {
            _pbiToolsRunner = pbiToolsRunner;
            _logger = logger;
        }

        [McpServerTool("validateTmdl")]
        [Description("Validates TMDL files for syntax and semantic errors.")]
        public async Task<ValidateTmdlResponse> ExecuteAsync(ValidateTmdlRequest request)
        {
            _logger.LogInformation("Starting TMDL validation");
            
            if (request.TmdlFiles == null)
                throw new McpException("TMDL files must be provided for validation.", McpErrorCode.InvalidParams);
                
            if (request.TmdlFiles.Count == 0)
                throw new McpException("At least one TMDL file must be provided for validation.", McpErrorCode.InvalidParams);
                
            try
            {
                var result = await _pbiToolsRunner.ValidateAsync(request.TmdlFiles);
                return ValidateTmdlResponse.FromValidationResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TMDL validation failed: {Message}", ex.Message);
                throw new McpException("TMDL validation failed: " + ex.Message, ex, McpErrorCode.InternalError);
            }
        }
    }
}