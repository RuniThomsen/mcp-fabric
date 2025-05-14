using System.Collections.Generic;
using System.Threading.Tasks;
using ModelContextProtocol;
using Moq;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using SemanticModelMcpServer.Services;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    public class McpLifecycleComplianceTests
    {
        [Fact]
        public async Task CreateUpdateRefresh_ToolSequence_ShouldWorkTogether()
        {
            // Create mocks
            var mockFabricClient = new Mock<IFabricClient>();
            
            // Setup mock responses
            mockFabricClient
                .Setup(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()))
                .ReturnsAsync("test-model-id");
                
            mockFabricClient
                .Setup(c => c.UpdateSemanticModelAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(true);
                
            mockFabricClient
                .Setup(c => c.RefreshSemanticModelAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            
            // Create sample TMDL files
            var tmdlFiles = new Dictionary<string, string>
            {
                { "model.tmdl", "model Test {}" },
                { "tables/Customer.tmdl", "table Customer {}" }
            };
            
            // Step 1: Create the semantic model
            var createTool = new Tools.CreateSemanticModelTool(
                Mock.Of<Microsoft.Extensions.Logging.ILogger<Tools.CreateSemanticModelTool>>(),
                mockFabricClient.Object
            );
            
            var createRequest = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                Description = "Test model for MCP compliance",
                TmdlFiles = tmdlFiles
            };
            
            var createResponse = await createTool.ExecuteAsync(createRequest);
            
            // Verify create result
            Assert.NotNull(createResponse);
            Assert.Equal("test-model-id", createResponse.ModelId);
            Assert.Equal("Created", createResponse.Status);
            
            // Step 2: Update the semantic model
            var updateTool = new Tools.UpdateSemanticModelTool(mockFabricClient.Object);
            
            var updateRequest = new UpdateSemanticModelRequest
            {
                ModelId = createResponse.ModelId,
                TmdlFiles = new Dictionary<string, string>(tmdlFiles)
                {
                    ["tables/Product.tmdl"] = "table Product {}"
                }
            };
            
            var updateResponse = await updateTool.ExecuteAsync(updateRequest);
            
            // Verify update result
            Assert.NotNull(updateResponse);
            Assert.Equal("Updated", updateResponse.Status);
            
            // Step 3: Refresh the semantic model
            var refreshTool = new Tools.RefreshTool(
                mockFabricClient.Object,
                Mock.Of<Microsoft.Extensions.Logging.ILogger<Tools.RefreshTool>>()
            );
            
            var refreshRequest = new RefreshSemanticModelRequest
            {
                ModelId = createResponse.ModelId,
                Type = "Full"
            };
            
            var refreshResponse = await refreshTool.ExecuteAsync(refreshRequest);
            
            // Verify refresh result
            Assert.NotNull(refreshResponse);
            Assert.Equal("Refreshing", refreshResponse.Status);
            
            // Verify that the client methods were called with expected parameters
            mockFabricClient.Verify(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()), Times.Once);
            mockFabricClient.Verify(c => c.UpdateSemanticModelAsync("test-model-id", It.IsAny<Dictionary<string, string>>()), Times.Once);
            mockFabricClient.Verify(c => c.RefreshSemanticModelAsync("test-model-id", "Full"), Times.Once);
        }
        
        [Fact]
        public void ValidationErrors_ShouldUseCorrectMcpErrorCodes()
        {
            // Check that validation errors use the correct MCP error codes
            
            // Create a tool instance
            var mockFabricClient = new Mock<IFabricClient>();
            var mockLogger = Mock.Of<Microsoft.Extensions.Logging.ILogger<Tools.CreateSemanticModelTool>>();
            var tool = new Tools.CreateSemanticModelTool(mockLogger, mockFabricClient.Object);
            
            // Test cases for validation errors
            var testCases = new[]
            {
                new { 
                    Request = new CreateSemanticModelRequest { Name = "", TmdlFiles = null }, 
                    ExpectedMessage = "Model name is required", 
                    ExpectedCode = McpErrorCode.InvalidParams 
                },
                new { 
                    Request = new CreateSemanticModelRequest { Name = "Valid", TmdlFiles = null }, 
                    ExpectedMessage = "At least one TMDL file is required", 
                    ExpectedCode = McpErrorCode.InvalidParams 
                }
            };
            
            // Run each test case
            foreach (var testCase in testCases)
            {
                var exception = Assert.ThrowsAsync<McpException>(
                    () => tool.ExecuteAsync(testCase.Request)
                );
                
                Assert.Equal(testCase.ExpectedCode, exception.Result.ErrorCode);
                Assert.Contains(testCase.ExpectedMessage, exception.Result.Message);
            }
            
            // Ensure no client calls were made during validation
            mockFabricClient.Verify(c => c.CreateSemanticModelAsync(It.IsAny<CreateSemanticModelRequest>()), Times.Never);
        }
    }
}
