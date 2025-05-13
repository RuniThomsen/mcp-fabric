using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpServerToolType]
    public class UpdateSemanticModelTool
    {
        private readonly IFabricClient _fabricClient;

        public UpdateSemanticModelTool(IFabricClient fabricClient)
        {
            _fabricClient = fabricClient;
        }

        [McpServerTool("updateSemanticModel")]
        [Description("Updates an existing semantic model in Fabric with new TMDL files.")]
        public async Task<UpdateSemanticModelResponse> ExecuteAsync(UpdateSemanticModelRequest request)
        {
            // Validate the request
            if (string.IsNullOrEmpty(request.ModelId))
            {
                throw new McpException("Model ID is required.", McpErrorCode.InvalidParams);
            }
            
            if (request.TmdlFiles == null || request.TmdlFiles.Count == 0)
            {
                throw new McpException("At least one TMDL file is required.", McpErrorCode.InvalidParams);
            }

            try
            {
                // Call the Fabric API to update the semantic model
                var success = await _fabricClient.UpdateSemanticModelAsync(request.ModelId, request.TmdlFiles);

                return new UpdateSemanticModelResponse
                {
                    Status = success ? "Updated" : "Failed",
                    UpdatedDetails = $"Semantic model {request.ModelId} update " + (success ? "completed successfully" : "failed")
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new McpException($"Failed to update semantic model {request.ModelId}: {ex.Message}", ex, McpErrorCode.InternalError);
            }
            catch (HttpRequestException ex)
            {
                throw new McpException($"Failed to communicate with Fabric API: {ex.Message}", ex, McpErrorCode.InternalError);
            }
            catch (Exception ex)
            {
                throw new McpException($"An unexpected error occurred while updating semantic model {request.ModelId}: {ex.Message}", ex, McpErrorCode.InternalError);
            }
        }
    }
}