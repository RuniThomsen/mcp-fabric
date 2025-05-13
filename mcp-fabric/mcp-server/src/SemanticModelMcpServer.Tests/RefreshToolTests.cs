using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests
{
    public class RefreshToolTests
    {
        private readonly Mock<ILogger<RefreshTool>> _mockLogger;
        private readonly Mock<IFabricClient> _mockFabricClient;
        private readonly RefreshTool _refreshTool;        public RefreshToolTests()
        {
            _mockLogger = new Mock<ILogger<RefreshTool>>();
            _mockFabricClient = new Mock<IFabricClient>();
            _refreshTool = new RefreshTool(_mockFabricClient.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidModelId_ShouldReturnRefreshingStatus()
        {
            // Arrange
            var request = new RefreshSemanticModelRequest
            {
                ModelId = "test-model-id",
                Type = "Full"
            };

            _mockFabricClient
                .Setup(x => x.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _refreshTool.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Refreshing", result.Status);
            Assert.Contains("started successfully", result.RefreshDetails);
            
            _mockFabricClient.Verify(
                x => x.RefreshSemanticModelAsync("test-model-id", "Full"),
                Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithFailedRefresh_ShouldReturnFailedStatus()
        {
            // Arrange
            var request = new RefreshSemanticModelRequest
            {
                ModelId = "test-model-id",
                Type = "Full"
            };

            _mockFabricClient
                .Setup(x => x.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _refreshTool.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Failed", result.Status);
            Assert.Contains("failed to start", result.RefreshDetails);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyModelId_ShouldReturnError()
        {
            // Arrange
            var request = new RefreshSemanticModelRequest
            {
                ModelId = "",
                Type = "Full"
            };

            // Act
            var result = await _refreshTool.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error", result.Status);
            Assert.Contains("Model ID is required", result.RefreshDetails);
            
            // Verify that RefreshSemanticModelAsync was not called
            _mockFabricClient.Verify(
                x => x.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithNullType_ShouldUseFullRefreshByDefault()
        {
            // Arrange
            var request = new RefreshSemanticModelRequest
            {
                ModelId = "test-model-id",
                Type = null
            };

            _mockFabricClient
                .Setup(x => x.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _refreshTool.ExecuteAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Refreshing", result.Status);
            
            _mockFabricClient.Verify(
                x => x.RefreshSemanticModelAsync("test-model-id", "Full"),
                Times.Once);
        }
    }
}
