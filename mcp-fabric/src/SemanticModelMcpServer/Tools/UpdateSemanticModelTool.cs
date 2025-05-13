using System;
using System.ComponentModel;
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
            if (string.IsNullOrEmpty(request.ModelId) || request.TmdlFiles == null || request.TmdlFiles.Count == 0)
            {
                return new UpdateSemanticModelResponse
                {
                    Status = "Error",
                    UpdatedDetails = "Model ID and TMDL files are required."
                };
            }

            // Call the Fabric API to update the semantic model
            var success = await _fabricClient.UpdateSemanticModelAsync(request.ModelId, request.TmdlFiles);

            return new UpdateSemanticModelResponse
            {
                Status = success ? "Updated" : "Failed",
                UpdatedDetails = $"Semantic model {request.ModelId} update " + (success ? "completed successfully" : "failed")
            };
        }
    }
}