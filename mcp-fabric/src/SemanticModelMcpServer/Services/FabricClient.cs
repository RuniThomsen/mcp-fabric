using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SemanticModelMcpServer.Models.Requests;

namespace SemanticModelMcpServer.Services
{
    public class FabricClient : IFabricClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FabricClient> _logger;
        private readonly string _baseUrl;

        public FabricClient(HttpClient httpClient, ILogger<FabricClient> logger, IConfiguration config = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _baseUrl = config?["FABRIC_API_URL"] ?? Environment.GetEnvironmentVariable("FABRIC_API_URL") ?? "https://api.powerbi.com";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public FabricClient(string baseAddress)
        {
            if (string.IsNullOrEmpty(baseAddress))
                throw new ArgumentNullException(nameof(baseAddress));

            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task AuthenticateAsync(string tenantId, string clientId, string clientSecret)
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var token = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://analysis.windows.net/powerbi/api/.default" }));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        // Best practice: support DefaultAzureCredential for local/dev/managed identity
        public async Task AuthenticateWithDefaultCredentialAsync()
        {
            var cred = new DefaultAzureCredential();
            var token = await cred.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://analysis.windows.net/powerbi/api/.default" }));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _httpClient.GetAsync(requestUri);
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return await _httpClient.PostAsync(requestUri, content);
        }

        public async Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return await _httpClient.DeleteAsync(requestUri);
        }

        public async Task<string> CreateSemanticModelAsync(CreateSemanticModelRequest request)
        {
            _logger.LogInformation("Creating semantic model: {0}", request.Name);
            
            // Pack TMDL files into a ZIP archive
            byte[] zipContent = ZipHelper.PackTmdl(request.TmdlFiles);
            
            // Create content for the request
            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(zipContent), "definitionFile", "model.zip" },
                { new StringContent(request.Name), "name" },
                { new StringContent(request.Description ?? string.Empty), "description" }
            };
            
            // Send request to Fabric API to create semantic model
            var response = await PostAsync("/v1.0/myorg/groups/{workspaceId}/semanticModels", content);
            response.EnsureSuccessStatusCode();
            
            // Parse response to get model ID
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            string modelId = jsonDoc.RootElement.GetProperty("id").GetString() ?? 
                throw new InvalidOperationException("Failed to get model ID from response");
            
            return modelId;
        }

        public async Task<bool> UpdateSemanticModelAsync(string modelId, Dictionary<string, string> tmdlFiles)
        {
            _logger.LogInformation("Updating semantic model: {0}", modelId);
            
            if (string.IsNullOrEmpty(modelId))
                throw new InvalidOperationException("Model ID is required");
                
            if (tmdlFiles == null || tmdlFiles.Count == 0)
                throw new InvalidOperationException("TMDL files are required");
                
            try
            {
                // Pack TMDL files into a ZIP archive
                byte[] zipContent = ZipHelper.PackTmdl(tmdlFiles);
                
                // Create content for the request
                var content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(zipContent), "definitionFile", "model.zip" }
                };
                
                // Send request to Fabric API to update semantic model
                var response = await PatchAsync($"/v1.0/myorg/semanticModels/{modelId}", content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating semantic model {0}: {1}", modelId, ex.Message);
                return false;
            }
        }

        public async Task<bool> RefreshSemanticModelAsync(string modelId, string refreshType = "Full")
        {
            _logger.LogInformation("Refreshing semantic model: {0} with type: {1}", modelId, refreshType);
            
            if (string.IsNullOrEmpty(modelId))
                throw new InvalidOperationException("Model ID is required");
                
            try
            {
                // Create content for refresh request
                var content = new StringContent(
                    JsonSerializer.Serialize(new { Type = refreshType }),
                    Encoding.UTF8,
                    "application/json");
                
                // Send request to trigger refresh
                var response = await PostAsync($"/v1.0/myorg/semanticModels/{modelId}/refreshes", content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing semantic model {0}: {1}", modelId, ex.Message);
                return false;
            }
        }
    }
}