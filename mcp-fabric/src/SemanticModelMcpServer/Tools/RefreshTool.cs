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
        public async Task<RefreshSemanticModelResponse> ExecuteAsync(RefreshSemanticModelRequest request)
        {
            _logger.LogInformation("Starting refresh for semantic model: {ModelId}, type: {Type}", request.ModelId, request.Type ?? "Full");
            
            if (string.IsNullOrEmpty(request.ModelId))
            {
                return new RefreshSemanticModelResponse
                {
                    Status = "Error",
                    RefreshDetails = "Model ID is required."
                };
            }
            
            // Trigger the refresh operation for the specified semantic model
            var success = await _fabricClient.RefreshSemanticModelAsync(request.ModelId, request.Type ?? "Full");
            
            return new RefreshSemanticModelResponse
            {
                Status = success ? "Refreshing" : "Failed",
                RefreshDetails = $"Refresh operation for model {request.ModelId} " + 
                               (success ? "started successfully" : "failed to start")
            };
        }
    }
}