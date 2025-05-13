using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using SemanticModelMcpServer.Models.Requests;

namespace SemanticModelMcpServer.Services
{
    public interface IFabricClient
    {
        Task AuthenticateAsync(string tenantId, string clientId, string clientSecret);
        Task<HttpResponseMessage> GetAsync(string requestUri);
        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content);
        Task<HttpResponseMessage> DeleteAsync(string requestUri);
        Task<string> CreateSemanticModelAsync(CreateSemanticModelRequest request);
        Task<bool> UpdateSemanticModelAsync(string modelId, Dictionary<string, string> tmdlFiles);
        Task<bool> RefreshSemanticModelAsync(string modelId, string refreshType = "Full");
    }
}
