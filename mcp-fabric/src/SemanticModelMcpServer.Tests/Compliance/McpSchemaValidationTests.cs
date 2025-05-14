using System;
using System.Collections.Generic;
using System.Text.Json;
using Json.Schema;
using System.Threading.Tasks;
using ModelContextProtocol;
using SemanticModelMcpServer.Models.Requests;
using SemanticModelMcpServer.Models.Responses;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    public class McpSchemaValidationTests
    {
        // This is a simplified schema for tool request parameters
        private readonly string _createSemanticModelRequestSchema = @"
        {
            ""type"": ""object"",
            ""required"": [""name"", ""tmdlFiles""],
            ""properties"": {
                ""name"": { ""type"": ""string"", ""minLength"": 1 },
                ""description"": { ""type"": ""string"" },
                ""tmdlFiles"": {
                    ""type"": ""object"",
                    ""additionalProperties"": { ""type"": ""string"" },
                    ""minProperties"": 1
                }
            }
        }";

        // This is a simplified schema for tool response objects
        private readonly string _createSemanticModelResponseSchema = @"
        {
            ""type"": ""object"",
            ""required"": [""modelId"", ""status""],
            ""properties"": {
                ""modelId"": { ""type"": ""string"" },
                ""status"": { ""type"": ""string"" }
            }
        }";

        // JSON-RPC schema for validating message structure
        private readonly string _jsonRpcSchema = @"
        {
            ""type"": ""object"",
            ""required"": [""jsonrpc""],
            ""properties"": {
                ""jsonrpc"": { ""enum"": [""2.0""] },
                ""id"": { ""type"": [""string"", ""number""] },
                ""method"": { ""type"": ""string"" },
                ""params"": { ""type"": ""object"" },
                ""result"": { ""type"": ""object"" },
                ""error"": {
                    ""type"": ""object"",
                    ""required"": [""code"", ""message""],
                    ""properties"": {
                        ""code"": { ""type"": ""number"" },
                        ""message"": { ""type"": ""string"" },
                        ""data"": { ""type"": ""object"" }
                    }
                }
            }
        }";

        [Fact]
        public void RequestSchemaShouldValidateCorrectly()
        {
            // Create valid request
            var validRequest = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                Description = "Test Description",
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model TestModel {}" }
                }
            };
              // Serialize to JSON with camelCase
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(validRequest, options);
            
            // Validate against schema
            Assert.True(ValidateJsonSchema(json, _createSemanticModelRequestSchema),
                "Valid request should pass schema validation");
            
            // Create invalid request (missing required name)
            var invalidRequest = new CreateSemanticModelRequest
            {
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model TestModel {}" }
                }
            };
            
            // Expect validation to fail
            var invalidJson = JsonSerializer.Serialize(invalidRequest, options);
            Assert.False(ValidateJsonSchema(invalidJson, _createSemanticModelRequestSchema),
                "Invalid request should fail schema validation");
        }
        
        [Fact]
        public void ResponseSchemaShouldValidateCorrectly()
        {
            // Create valid response
            var validResponse = new CreateSemanticModelResponse
            {
                ModelId = "test-id",
                Status = "Created"
            };
              // Serialize to JSON with camelCase
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var json = JsonSerializer.Serialize(validResponse, options);
            
            // Validate against schema
            Assert.True(ValidateJsonSchema(json, _createSemanticModelResponseSchema),
                "Valid response should pass schema validation");
            
            // Create invalid response (missing required modelId)
            var invalidResponse = new CreateSemanticModelResponse
            {
                Status = "Created"
            };
            
            // Expect validation to fail
            var invalidJson = JsonSerializer.Serialize(invalidResponse, options);
            Assert.False(ValidateJsonSchema(invalidJson, _createSemanticModelResponseSchema),
                "Invalid response should fail schema validation");
        }        [Fact]
        public void JsonSerializationShouldFollowMcpConventions()
        {
            // MCP requires camelCase property names in JSON
            
            // Create request and response objects
            var request = new CreateSemanticModelRequest
            {
                Name = "TestModel",
                Description = "Test description",
                TmdlFiles = new Dictionary<string, string>
                {
                    { "model.tmdl", "model TestModel {}" }
                }
            };
            
            var response = new CreateSemanticModelResponse
            {
                ModelId = "test-id",
                Status = "Created"
            };
            
            // Serialize to JSON with camelCase naming policy
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var requestJson = JsonSerializer.Serialize(request, options);
            var responseJson = JsonSerializer.Serialize(response, options);
            
            // Verify camelCase property names
            Assert.Contains("name", requestJson);
            Assert.Contains("description", requestJson);
            Assert.Contains("tmdlFiles", requestJson);
            Assert.DoesNotContain("Name", requestJson);
            Assert.DoesNotContain("Description", requestJson);
            Assert.DoesNotContain("TmdlFiles", requestJson);
            
            Assert.Contains("modelId", responseJson);
            Assert.Contains("status", responseJson);
            Assert.DoesNotContain("ModelId", responseJson);
            Assert.DoesNotContain("Status", responseJson);
        }

        [Fact]
        public void McpErrorCodesShouldBeWellDefined()
        {
            // Test that MCP error codes are properly defined
            
            // MCP standard error codes
            Assert.Equal(-32700, (int)McpErrorCode.ParseError);
            Assert.Equal(-32600, (int)McpErrorCode.InvalidRequest);
            Assert.Equal(-32601, (int)McpErrorCode.MethodNotFound);
            Assert.Equal(-32602, (int)McpErrorCode.InvalidParams);
            Assert.Equal(-32603, (int)McpErrorCode.InternalError);
            
            // Create MCP exceptions with different error codes
            var parseEx = new McpException("Parse error", McpErrorCode.ParseError);
            var invalidReqEx = new McpException("Invalid request", McpErrorCode.InvalidRequest);
            var notFoundEx = new McpException("Method not found", McpErrorCode.MethodNotFound);
            var invalidParamsEx = new McpException("Invalid parameters", McpErrorCode.InvalidParams);
            var internalEx = new McpException("Internal error", McpErrorCode.InternalError);
            
            // Verify the error codes were correctly set
            Assert.Equal(McpErrorCode.ParseError, parseEx.ErrorCode);
            Assert.Equal(McpErrorCode.InvalidRequest, invalidReqEx.ErrorCode);
            Assert.Equal(McpErrorCode.MethodNotFound, notFoundEx.ErrorCode);
            Assert.Equal(McpErrorCode.InvalidParams, invalidParamsEx.ErrorCode);
            Assert.Equal(McpErrorCode.InternalError, internalEx.ErrorCode);
        }

        [Fact]
        public void JsonRpcMessages_ShouldFollowJsonRpcFormat()
        {
            // Test that our JSON-RPC message structure is valid
            
            // Create a sample JSON-RPC request
            string jsonRpcRequest = @"
            {
                ""jsonrpc"": ""2.0"",
                ""id"": ""1"",
                ""method"": ""createSemanticModel"",
                ""params"": {
                    ""name"": ""TestModel"",
                    ""description"": ""Test description"",
                    ""tmdlFiles"": {
                        ""model.tmdl"": ""model TestModel {}""
                    }
                }
            }";
            
            // Validate against JSON-RPC schema
            Assert.True(ValidateJsonSchema(jsonRpcRequest, _jsonRpcSchema),
                "JSON-RPC request should follow the JSON-RPC 2.0 schema");
                
            // Create a sample JSON-RPC response
            string jsonRpcResponse = @"
            {
                ""jsonrpc"": ""2.0"",
                ""id"": ""1"",
                ""result"": {
                    ""modelId"": ""test-id"",
                    ""status"": ""Created""
                }
            }";
            
            // Validate against JSON-RPC schema
            Assert.True(ValidateJsonSchema(jsonRpcResponse, _jsonRpcSchema),
                "JSON-RPC response should follow the JSON-RPC 2.0 schema");
                
            // Create a sample JSON-RPC error response
            string jsonRpcError = @"
            {
                ""jsonrpc"": ""2.0"",
                ""id"": ""1"",
                ""error"": {
                    ""code"": -32602,
                    ""message"": ""Invalid parameters"",
                    ""data"": {
                        ""details"": ""Model name is required""
                    }
                }
            }";
            
            // Validate against JSON-RPC schema
            Assert.True(ValidateJsonSchema(jsonRpcError, _jsonRpcSchema),
                "JSON-RPC error should follow the JSON-RPC 2.0 schema");
        }

        private bool ValidateJsonSchema(string json, string schemaJson)
        {
            try
            {
                var schema = JsonSchema.FromText(schemaJson);
                var document = JsonDocument.Parse(json);
                var evaluationResults = schema.Evaluate(document.RootElement);
                return evaluationResults.IsValid;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Schema validation failed: " + ex.Message);
            }
        }
    }
}
