using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tools
{
    [McpServerToolType]
    public class DeploymentTool
    {
        private readonly IFabricClient _fabricClient;

        public DeploymentTool(IFabricClient fabricClient)
        {
            _fabricClient = fabricClient;
        }

        [McpServerTool("deploySemanticModel")]
        [Description("Deploys a semantic model to the specified target environment.")]
        public async Task DeploySemanticModelAsync(string modelId, string targetEnvironment)
        {
            // Validate parameters
            if (string.IsNullOrEmpty(modelId))
            {
                throw new McpException("Model ID is required.", McpErrorCode.InvalidParams);
            }
            
            if (string.IsNullOrEmpty(targetEnvironment))
            {
                throw new McpException("Target environment is required.", McpErrorCode.InvalidParams);
            }
            
            try
            {
                // Logic to deploy the semantic model to the specified environment
                var deploymentRequest = new
                {
                    ModelId = modelId,
                    TargetEnvironment = targetEnvironment
                };

                var content = new StringContent(JsonSerializer.Serialize(deploymentRequest), System.Text.Encoding.UTF8, "application/json");
                var response = await _fabricClient.PostAsync("/deployments", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = $"Deployment failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}";
                    throw new McpException(errorMessage, McpErrorCode.InternalError);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new McpException($"Failed to communicate with deployment service: {ex.Message}", ex, McpErrorCode.InternalError);
            }
            catch (McpException)
            {
                // Re-throw MCP exceptions as they are already properly formatted
                throw;
            }
            catch (Exception ex)
            {
                throw new McpException($"An unexpected error occurred during deployment: {ex.Message}", ex, McpErrorCode.InternalError);
            }
        }
    }
}