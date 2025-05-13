using System;
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
        public async Task DeploySemanticModelAsync(string modelId, string targetEnvironment)
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
                throw new Exception($"Deployment failed: {response.ReasonPhrase}");
            }
        }
    }
}