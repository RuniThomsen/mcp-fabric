using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests
{    public class McpExceptionHandlingTests
    {
        #region CreateSemanticModelTool Tests
        
        [Fact]
        public async Task CreateSemanticModelTool_EmptyModelName_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            var request = new CreateSemanticModelRequest { Name = string.Empty };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("Model name is required", exception.Message);
        }        [Fact]
        public async Task CreateSemanticModelTool_NoTmdlFiles_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            var request = new CreateSemanticModelRequest 
            { 
                Name = "TestModel", 
                TmdlFiles = null 
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("At least one TMDL file is required", exception.Message);        }
        
        [Fact]
        public async Task CreateSemanticModelTool_ClientThrowsInvalidOperation_ShouldRethrowAsMcpException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            var request = new CreateSemanticModelRequest 
            { 
                Name = "TestModel", 
                TmdlFiles = new Dictionary<string, string> { { "test.tmdl", "content" } }
            };
            
            mockFabricClient
                .Setup(x => x.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ThrowsAsync(new InvalidOperationException("Test exception"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("Failed to create semantic model", exception.Message);
            Assert.IsType<InvalidOperationException>(exception.InnerException);
        }

        #endregion

        #region UpdateSemanticModelTool Tests

        [Fact]
        public async Task UpdateSemanticModelTool_EmptyModelId_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new UpdateSemanticModelTool(mockFabricClient.Object);
            var request = new UpdateSemanticModelRequest { ModelId = string.Empty };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("Model ID is required", exception.Message);
        }

        [Fact]
        public async Task UpdateSemanticModelTool_ClientThrowsHttpRequest_ShouldRethrowAsMcpException()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new UpdateSemanticModelTool(mockFabricClient.Object);
            var request = new UpdateSemanticModelRequest 
            { 
                ModelId = "test-id", 
                TmdlFiles = new Dictionary<string, string> { { "test.tmdl", "content" } }
            };

            mockFabricClient
                .Setup(x => x.UpdateSemanticModelAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new HttpRequestException("Connection error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("Failed to communicate with Fabric API", exception.Message);
            Assert.IsType<HttpRequestException>(exception.InnerException);
        }

        #endregion

        #region RefreshTool Tests
        
        [Fact]
        public async Task RefreshTool_EmptyModelId_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var mockLogger = new Mock<ILogger<RefreshTool>>();
            var tool = new RefreshTool(mockFabricClient.Object, mockLogger.Object);
            var request = new RefreshSemanticModelRequest { ModelId = string.Empty };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("Model ID is required", exception.Message);
        }

        [Fact]
        public async Task RefreshTool_ClientThrowsException_ShouldRethrowAsMcpException()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var mockLogger = new Mock<ILogger<RefreshTool>>();
            var tool = new RefreshTool(mockFabricClient.Object, mockLogger.Object);
            var request = new RefreshSemanticModelRequest { ModelId = "test-id" };

            mockFabricClient
                .Setup(x => x.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("General error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));            Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("An unexpected error occurred during refresh", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }
        
        #endregion

        #region ValidateTmdlTool Tests
        
        [Fact]
        public async Task ValidateTmdlTool_NullTmdlFiles_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockPbiToolsRunner = new Mock<IPbiToolsRunner>();
            var mockLogger = new Mock<ILogger<ValidateTmdlTool>>();
            var tool = new ValidateTmdlTool(mockPbiToolsRunner.Object, mockLogger.Object);
            var request = new Models.Requests.ValidateTmdlRequest { TmdlFiles = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ValidateAsync(request));
              Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("TMDL files must be provided for validation", exception.Message);
        }
        
        [Fact]
        public async Task ValidateTmdlTool_RunnerThrowsException_ShouldRethrowAsMcpException()
        {
            // Arrange
            var mockPbiToolsRunner = new Mock<IPbiToolsRunner>();
            var mockLogger = new Mock<ILogger<ValidateTmdlTool>>();
            var tool = new ValidateTmdlTool(mockPbiToolsRunner.Object, mockLogger.Object);
            var tmdlFiles = new Dictionary<string, string> { { "test.tmdl", "content" } };
            var request = new Models.Requests.ValidateTmdlRequest { TmdlFiles = tmdlFiles };

            mockPbiToolsRunner
                .Setup(x => x.ValidateAsync(It.IsAny<Dictionary<string, string>>()))
                .ThrowsAsync(new Exception("Validation error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                async () => await tool.ValidateAsync(request));
              Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("TMDL validation failed", exception.Message);
            Assert.IsType<Exception>(exception.InnerException);
        }
        
        #endregion

        #region DeploymentTool Tests
        
        [Fact]
        public void DeploymentTool_EmptyModelId_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new DeploymentTool(mockFabricClient.Object);
            var request = new Models.Requests.DeploymentRequest
            {
                ModelId = "",
                TargetEnvironment = "test-env"
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
              Assert.Equal(McpErrorCode.InvalidParams, exception.Result.ErrorCode);
            Assert.Contains("Model ID is required", exception.Result.Message);
        }
        
        [Fact]
        public void DeploymentTool_EmptyTargetEnvironment_ShouldThrowMcpExceptionWithInvalidParams()
        {
            // Arrange
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new DeploymentTool(mockFabricClient.Object);
            var request = new Models.Requests.DeploymentRequest
            {
                ModelId = "test-model",
                TargetEnvironment = ""
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<McpException>(
                async () => await tool.ExecuteAsync(request));
            
            Assert.Equal(McpErrorCode.InvalidParams, exception.Result.ErrorCode);
            Assert.Contains("Target environment is required", exception.Result.Message);
        }

        #endregion
    }
}
