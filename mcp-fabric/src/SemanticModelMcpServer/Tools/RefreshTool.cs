using System;
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
    public class RefreshTool
    {
        private readonly IFabricClient _fabricClient;
        private readonly ILogger<RefreshTool> _logger;
    
        public RefreshTool(IFabricClient fabricClient, ILogger<RefreshTool> logger)
        {
            _fabricClient = fabricClient;
            _logger = logger;
        }

        [McpServerTool("refreshSemanticModel")]
        [Description("Refreshes a semantic model in Fabric to update its data.")]
        public async Task<RefreshSemanticModelResponse> ExecuteAsync(RefreshSemanticModelRequest request)
        {
            _logger.LogInformation("Starting refresh for semantic model: {ModelId}, type: {Type}", request.ModelId, request.Type ?? "Full");
            
            if (string.IsNullOrEmpty(request.ModelId))
            {
                throw new McpException("Model ID is required.", McpErrorCode.InvalidParams);
            }
            
            try
            {
                // Trigger the refresh operation for the specified semantic model
                var success = await _fabricClient.RefreshSemanticModelAsync(request.ModelId, request.Type ?? "Full");
                
                return new RefreshSemanticModelResponse
                {
                    Status = success ? "Refreshing" : "Failed",
                    RefreshDetails = $"Refresh operation for model {request.ModelId} " + 
                                   (success ? "started successfully" : "failed to start")
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to refresh model {ModelId}: {Message}", request.ModelId, ex.Message);
                throw new McpException($"Failed to refresh model: {ex.Message}", ex, McpErrorCode.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during model refresh: {Message}", ex.Message);
                throw new McpException($"An unexpected error occurred during refresh: {ex.Message}", ex, McpErrorCode.InternalError);
            }
        }
    }
}