using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests
{
    public class DeploymentToolTests
    {
        private readonly Mock<IFabricClient> _mockFabricClient;
        private readonly DeploymentTool _deploymentTool;        public DeploymentToolTests()
        {
            _mockFabricClient = new Mock<IFabricClient>();
            _deploymentTool = new DeploymentTool(_mockFabricClient.Object);
        }

        [Fact]
        public async Task DeploySemanticModelAsync_SuccessfulDeployment_ShouldNotThrow()
        {
            // Arrange
            var modelId = "test-model-id";
            var targetEnvironment = "Production";

            _mockFabricClient
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await _deploymentTool.DeploySemanticModelAsync(modelId, targetEnvironment);

            // Verify that PostAsync was called with the correct path
            _mockFabricClient.Verify(
                x => x.PostAsync("/deployments", It.IsAny<HttpContent>()),
                Times.Once);
        }

        [Fact]
        public async Task DeploySemanticModelAsync_FailedDeployment_ShouldThrowException()
        {
            // Arrange
            var modelId = "test-model-id";
            var targetEnvironment = "Production";

            _mockFabricClient
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "Invalid model ID"
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _deploymentTool.DeploySemanticModelAsync(modelId, targetEnvironment));

            Assert.Contains("Deployment failed", exception.Message);
            Assert.Contains("Invalid model ID", exception.Message);
        }

        [Fact]
        public async Task DeploySemanticModelAsync_NullModelId_ShouldPassNullToClient()
        {
            // Arrange
            string modelId = null;
            var targetEnvironment = "Production";

            _mockFabricClient
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await _deploymentTool.DeploySemanticModelAsync(modelId, targetEnvironment);

            // Verify that PostAsync was called
            _mockFabricClient.Verify(
                x => x.PostAsync("/deployments", It.IsAny<HttpContent>()),
                Times.Once);
        }

        [Fact]
        public async Task DeploySemanticModelAsync_EmptyTargetEnvironment_ShouldPassEmptyStringToClient()
        {
            // Arrange
            var modelId = "test-model-id";
            string targetEnvironment = "";

            _mockFabricClient
                .Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await _deploymentTool.DeploySemanticModelAsync(modelId, targetEnvironment);

            // Verify that PostAsync was called
            _mockFabricClient.Verify(
                x => x.PostAsync("/deployments", It.IsAny<HttpContent>()),
                Times.Once);
        }
    }
}
