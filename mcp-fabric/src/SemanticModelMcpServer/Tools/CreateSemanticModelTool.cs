using System;
using System.Collections.Generic;
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
        private readonly FabricClient _fabricClient;

        public CreateSemanticModelTool(ILogger<CreateSemanticModelTool> logger, FabricClient fabricClient)
        {
            _logger = logger;
            _fabricClient = fabricClient;
        }

        [McpServerTool("createSemanticModel")]
        public async Task<CreateSemanticModelResponse> ExecuteAsync(CreateSemanticModelRequest request)
        {
            _logger.LogInformation("Starting the creation of semantic model: {ModelName}", request.Name);

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name) || request.TmdlFiles == null || request.TmdlFiles.Count == 0)
            {
                throw new ArgumentException("Invalid request parameters.");
            }

            // Create the semantic model
            var modelId = await _fabricClient.CreateSemanticModelAsync(request);
            
            _logger.LogInformation("Semantic model created successfully with ID: {ModelId}", modelId);

            return new CreateSemanticModelResponse
            {
                ModelId = modelId,
                Status = "Created"
            };
        }
    }
}