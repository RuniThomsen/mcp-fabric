using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpServerToolType]
    public class CreateSemanticModelTool
    {
        private readonly ILogger<CreateSemanticModelTool> _logger;
        private readonly IFabricClient _fabricClient;

        public CreateSemanticModelTool(ILogger<CreateSemanticModelTool> logger, IFabricClient fabricClient)
        {
            _logger = logger;
            _fabricClient = fabricClient;
        }

        [McpServerTool("createSemanticModel")]
        [Description("Creates a new semantic model in Fabric from TMDL files.")]
        public async Task<CreateSemanticModelResponse> ExecuteAsync(CreateSemanticModelRequest request)
        {
            _logger.LogInformation("Starting the creation of semantic model: {ModelName}", request.Name);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new McpException("Model name is required.", McpErrorCode.InvalidParams);
            }
            
            if (request.TmdlFiles == null || request.TmdlFiles.Count == 0)
            {
                throw new McpException("At least one TMDL file is required.", McpErrorCode.InvalidParams);
            }

            try
            {
                // Create the semantic model
                var modelId = await _fabricClient.CreateSemanticModelAsync(request);
                
                _logger.LogInformation("Semantic model created successfully with ID: {ModelId}", modelId);

                return new CreateSemanticModelResponse
                {
                    ModelId = modelId,
                    Status = "Created"
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to create semantic model due to invalid operation: {Message}", ex.Message);
                throw new McpException("Failed to create semantic model: " + ex.Message, ex, McpErrorCode.InternalError);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to create semantic model due to HTTP error: {Message}", ex.Message);
                throw new McpException("Failed to communicate with Fabric API: " + ex.Message, ex, McpErrorCode.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating semantic model: {Message}", ex.Message);
                throw new McpException("An unexpected error occurred: " + ex.Message, ex, McpErrorCode.InternalError);
            }
        }
    }
}