using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    public class ToolExecutionTests
    {
        [Fact]
        public async Task Tool_InvalidParameters_ShouldThrowProperMcpException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Create request with empty name
            var invalidRequest = new CreateSemanticModelRequest { Name = "" };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                () => tool.ExecuteAsync(invalidRequest));
            
            // Verify exception follows MCP standards
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("Model name is required", exception.Message);
        }
        
        [Fact]
        public async Task Tool_MissingTmdlFiles_ShouldThrowProperMcpException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Create request with missing TMDL files
            var invalidRequest = new CreateSemanticModelRequest 
            { 
                Name = "ValidName",
                TmdlFiles = null
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                () => tool.ExecuteAsync(invalidRequest));
            
            // Verify exception follows MCP standards
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            Assert.Contains("At least one TMDL file is required", exception.Message);
        }
        
        [Fact]
        public async Task Tool_WithHttpRequestException_ShouldConvertToMcpException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Setup mock client to throw HttpRequestException
            mockFabricClient
                .Setup(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ThrowsAsync(new System.Net.Http.HttpRequestException("Connection error"));
            
            // Create valid request
            var request = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model content" }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                () => tool.ExecuteAsync(request));
            
            // Verify exception follows MCP standards for network errors
            Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("Failed to communicate with Fabric API", exception.Message);
        }
        
        [Fact]
        public async Task Tool_WithGenericException_ShouldConvertToMcpException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Setup mock client to throw a generic exception
            mockFabricClient
                .Setup(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ThrowsAsync(new Exception("Generic error"));
            
            // Create valid request
            var request = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model content" }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                () => tool.ExecuteAsync(request));
            
            // Verify exception follows MCP standards for unexpected errors
            Assert.Equal(McpErrorCode.InternalError, exception.ErrorCode);
            Assert.Contains("An unexpected error occurred", exception.Message);
        }
        
        [Fact]
        public async Task Tool_ValidRequest_ShouldReturnSuccessfulResponse()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Configure mock
            string expectedModelId = "test-model-id";
            mockFabricClient
                .Setup(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ReturnsAsync(expectedModelId);
            
            // Create valid request
            var request = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                Description = "Test Description",
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model TestModel {}" }
                }
            };
            
            // Act
            var response = await tool.ExecuteAsync(request);
            
            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedModelId, response.ModelId);
            Assert.Equal("Created", response.Status);
        }
        
        [Fact]
        public async Task ToolPipeline_ShouldHandleParameterValidation()
        {
            // Test that tools properly validate parameters before processing
            
            // Arrange - Create service provider with all required services
            var services = new ServiceCollection();
            
            // Register mock logger factory
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory
                .Setup(f => f.CreateLogger(It.IsAny<string>()))
                .Returns(Mock.Of<ILogger>());
                
            services.AddSingleton(mockLoggerFactory.Object);
            
            // Register mock fabric client
            var mockFabricClient = new Mock<IFabricClient>();
            mockFabricClient
                .Setup(c => c.UpdateSemanticModelAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);
            
            services.AddSingleton(mockFabricClient.Object);
            
            // Register tool
            services.AddTransient<UpdateSemanticModelTool>();
            
            var serviceProvider = services.BuildServiceProvider();
            
            // Get the tool 
            var tool = serviceProvider.GetRequiredService<UpdateSemanticModelTool>();
            
            // Test invalid request
            var invalidRequest = new UpdateSemanticModelRequest
            {
                ModelId = "",  // Empty ID
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model content" }
                }
            };
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<McpException>(
                () => tool.ExecuteAsync(invalidRequest));
            
            // Verify proper validation error
            Assert.Equal(McpErrorCode.InvalidParams, exception.ErrorCode);
            
            // Verify that the client method was never called due to validation failure
            mockFabricClient.Verify(
                c => c.UpdateSemanticModelAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()),
                Times.Never
            );
        }
    }
}
