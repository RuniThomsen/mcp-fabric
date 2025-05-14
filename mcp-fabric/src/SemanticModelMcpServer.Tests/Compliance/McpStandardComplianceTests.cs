using System;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    public class McpStandardComplianceTests
    {
        [Fact]
        public void ToolRegistration_ShouldHaveRequiredMetadata()
        {
            // Check if our tools have the required attributes
            var createToolType = typeof(CreateSemanticModelTool);
            var updateToolType = typeof(UpdateSemanticModelTool);
            var refreshToolType = typeof(RefreshTool);
            
            // Verify tool type attributes
            Assert.True(Attribute.IsDefined(createToolType, typeof(ModelContextProtocol.McpServerToolTypeAttribute)),
                "CreateSemanticModelTool should have McpServerToolTypeAttribute");

            Assert.True(Attribute.IsDefined(updateToolType, typeof(ModelContextProtocol.McpServerToolTypeAttribute)),
                "UpdateSemanticModelTool should have McpServerToolTypeAttribute");
            
            Assert.True(Attribute.IsDefined(refreshToolType, typeof(ModelContextProtocol.McpServerToolTypeAttribute)),
                "RefreshTool should have McpServerToolTypeAttribute");
            
            // Verify tool method attributes
            var createExecuteMethod = createToolType.GetMethod("ExecuteAsync");
            var updateExecuteMethod = updateToolType.GetMethod("ExecuteAsync");
            var refreshExecuteMethod = refreshToolType.GetMethod("ExecuteAsync");
            
            Assert.True(Attribute.IsDefined(createExecuteMethod, typeof(ModelContextProtocol.McpServerToolAttribute)),
                "ExecuteAsync method should have McpServerToolAttribute");
            
            // Check if tool names follow MCP conventions (camelCase)
            var createToolAttr = createExecuteMethod.GetCustomAttribute<ModelContextProtocol.McpServerToolAttribute>();
            Assert.Equal("createSemanticModel", createToolAttr.Name);
            
            // Check that Description attribute is provided
            var hasDescription = Attribute.IsDefined(createExecuteMethod, typeof(System.ComponentModel.DescriptionAttribute));
            Assert.True(hasDescription, "Tools should have description attribute");
        }
        
        [Fact]
        public void ToolImplementation_ShouldValidateParameters()
        {
            // Create test instances
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Test with invalid input - null name
            var request = new CreateSemanticModelRequest { Name = null };
            
            // Should throw McpException with proper error code
            var exception = Assert.ThrowsAsync<ModelContextProtocol.McpException>(
                () => tool.ExecuteAsync(request));
            
            Assert.Equal(ModelContextProtocol.McpErrorCode.InvalidParams, exception.Result.ErrorCode);
        }
          [Fact]
        public void ResponseFormats_ShouldFollowMcpGuidelines()
        {
            // Verify response formats follow MCP guidelines
            
            // Create response objects
            var createResponse = new Models.Responses.CreateSemanticModelResponse
            {
                ModelId = "test-id",
                Status = "Created"
            };
            
            var updateResponse = new Models.Responses.UpdateSemanticModelResponse
            {
                Status = "Updated",
                UpdatedDetails = "Model updated successfully"
            };
            
            // Serialize to check format with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string createJson = JsonSerializer.Serialize(createResponse, options);
            string updateJson = JsonSerializer.Serialize(updateResponse, options);
            
            // MCP requires camelCase properties in JSON
            Assert.Contains("modelId", createJson);
            Assert.Contains("status", createJson);
            Assert.Contains("status", updateJson);
            Assert.Contains("updatedDetails", updateJson);
        }
        
        [Fact]
        public void ExceptionHandling_ShouldThrowMcpExceptions()
        {
            // Test that our code correctly maps exceptions to McpExceptions
            
            // Create test instances
            var mockLogger = new Mock<ILogger<CreateSemanticModelTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            
            // Setup mock to throw standard exception
            mockFabricClient
                .Setup(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ThrowsAsync(new InvalidOperationException("Test error"));
                
            var tool = new CreateSemanticModelTool(mockLogger.Object, mockFabricClient.Object);
            
            // Create valid request
            var request = new CreateSemanticModelRequest
            { 
                Name = "Test",
                TmdlFiles = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "model.tmdl", "content" }
                }
            };
            
            // Should throw McpException with InternalError code 
            var exception = Assert.ThrowsAsync<ModelContextProtocol.McpException>(
                () => tool.ExecuteAsync(request));
            
            Assert.Equal(ModelContextProtocol.McpErrorCode.InternalError, exception.Result.ErrorCode);
        }
        
        [Fact]
        public void RequestParameters_ShouldBeValidated()
        {
            // Ensure that input validation is happening correctly
            
            // Create mocks
            var mockLogger = new Mock<ILogger<RefreshTool>>();
            var mockFabricClient = new Mock<IFabricClient>();
            var tool = new RefreshTool(mockFabricClient.Object, mockLogger.Object);
            
            // Invalid case: Empty model ID
            var request1 = new Models.Requests.RefreshSemanticModelRequest { ModelId = "" };
            var ex1 = Assert.ThrowsAsync<ModelContextProtocol.McpException>(
                () => tool.ExecuteAsync(request1));
            Assert.Equal(ModelContextProtocol.McpErrorCode.InvalidParams, ex1.Result.ErrorCode);
            
            // Valid case: Has model ID
            var request2 = new Models.Requests.RefreshSemanticModelRequest { ModelId = "valid-id" };
            mockFabricClient
                .Setup(c => c.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
                
            var response = tool.ExecuteAsync(request2);
            Assert.NotNull(response);
        }
    }
}
