using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests
{
    public class UpdateSemanticModelToolTests
    {
        private readonly Mock<IFabricClient> _mockFabricClient;
        private readonly UpdateSemanticModelTool _tool;

        public UpdateSemanticModelToolTests()
        {
            _mockFabricClient = new Mock<IFabricClient>();
            _tool = new UpdateSemanticModelTool(_mockFabricClient.Object);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsError_WhenRequestIsInvalid()
        {
            var req = new UpdateSemanticModelRequest { ModelId = null, TmdlFiles = null };
            var result = await _tool.ExecuteAsync(req);
            Assert.Equal("Error", result.Status);
            Assert.Contains("required", result.UpdatedDetails);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsUpdated_WhenClientReturnsTrue()
        {
            var req = new UpdateSemanticModelRequest
            {
                ModelId = "id",
                TmdlFiles = new Dictionary<string, string> { { "model.tmdl", "model X {}" } }
            };
            _mockFabricClient.Setup(x => x.UpdateSemanticModelAsync("id", req.TmdlFiles)).ReturnsAsync(true);
            var result = await _tool.ExecuteAsync(req);
            Assert.Equal("Updated", result.Status);
            Assert.Contains("completed successfully", result.UpdatedDetails);
        }

        [Fact]
        public async Task ExecuteAsync_ReturnsFailed_WhenClientReturnsFalse()
        {
            var req = new UpdateSemanticModelRequest
            {
                ModelId = "id",
                TmdlFiles = new Dictionary<string, string> { { "model.tmdl", "model X {}" } }
            };
            _mockFabricClient.Setup(x => x.UpdateSemanticModelAsync("id", req.TmdlFiles)).ReturnsAsync(false);
            var result = await _tool.ExecuteAsync(req);
            Assert.Equal("Failed", result.Status);
            Assert.Contains("failed", result.UpdatedDetails);
        }
    }
}
