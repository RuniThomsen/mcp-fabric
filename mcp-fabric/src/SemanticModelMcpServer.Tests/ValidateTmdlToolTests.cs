using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;
using SemanticModelMcpServer.Models.Requests;

namespace SemanticModelMcpServer.Tests
{
    // Test implementation of IPbiToolsRunner for testing
    public class TestPbiToolsRunner : IPbiToolsRunner
    {        
        public bool ShouldSucceed { get; set; } = true;
        public List<ValidationError> ErrorMessages { get; set; } = new List<ValidationError>();
        public bool ShouldThrowException { get; set; } = false;
        public string ExceptionMessage { get; set; } = "PBI Tools not found or failed to execute";
        public Dictionary<string, string> LastFilesValidated { get; private set; }
        public int CallCount { get; set; } = 0;

        public Task<ValidationResult> ValidateAsync(Dictionary<string, string> tmdlFiles)
        {
            CallCount++;
            LastFilesValidated = tmdlFiles;
            
            if (ShouldThrowException)
            {
                throw new Exception(ExceptionMessage);
            }
            
            // Validate TMDL files are not empty
            if (tmdlFiles == null || tmdlFiles.Count == 0)
            {
                return Task.FromResult(new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<ValidationError> { new ValidationError { Message = "No TMDL files provided for validation" } }
                });
            }

            return Task.FromResult(new ValidationResult
            {
                IsValid = ShouldSucceed,
                Errors = ErrorMessages
            });
        }
    }    

    public class ValidateTmdlToolTests
    {
        private readonly TestPbiToolsRunner _testRunner;
        private readonly ValidateTmdlTool _validateTmdlTool;
        private readonly Mock<ILogger<ValidateTmdlTool>> _mockLogger;

        public ValidateTmdlToolTests()
        {
            _testRunner = new TestPbiToolsRunner();
            _mockLogger = new Mock<ILogger<ValidateTmdlTool>>();
            _validateTmdlTool = new ValidateTmdlTool(_testRunner, _mockLogger.Object);
        }        

        [Fact]
        public async Task ValidateAsync_ShouldReturnValidResult_WhenTmdlIsValid()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>
            {
                { "model.tmdl", "model TestModel { }" },
                { "tables/table1.tmdl", "table Table1 { }" }
            };

            _testRunner.ShouldSucceed = true;
            _testRunner.ErrorMessages = new List<ValidationError>();

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var result = await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
            Assert.Equal(2, _testRunner.LastFilesValidated.Count);
            Assert.True(_testRunner.LastFilesValidated.ContainsKey("model.tmdl"));
            Assert.True(_testRunner.LastFilesValidated.ContainsKey("tables/table1.tmdl"));
        }        

        [Fact]
        public async Task ValidateAsync_ShouldReturnInvalidResult_WhenTmdlHasErrors()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>
            {
                { "model.tmdl", "model TestModel { /* Invalid syntax */ " }, // Missing closing brace
                { "tables/table1.tmdl", "table Table1 { }" }
            };

            _testRunner.ShouldSucceed = false;
            _testRunner.ErrorMessages = new List<ValidationError> { 
                new ValidationError { Message = "Error: Syntax error at line 1: Missing closing brace" } 
            };

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var result = await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Syntax error", result.Errors[0].Message);
            Assert.Equal(2, _testRunner.LastFilesValidated.Count);
        }        

        [Fact]
        public async Task ValidateAsync_ShouldHandleExceptions_FromPbiToolsRunner()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>
            {
                { "model.tmdl", "model TestModel { }" }
            };

            _testRunner.ShouldThrowException = true;
            _testRunner.ExceptionMessage = "PBI Tools not found or failed to execute";

            // Act & Assert
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var exception = await Assert.ThrowsAsync<ModelContextProtocol.McpException>(() => _validateTmdlTool.ValidateAsync(request));
            Assert.Contains("PBI Tools", exception.Message);
            Assert.Equal(ModelContextProtocol.McpErrorCode.InternalError, exception.ErrorCode);
        }        

        [Fact]
        public async Task ValidateAsync_ShouldHandleEmptyTmdlFiles()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>();

            // Act & Assert
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var exception = await Assert.ThrowsAsync<ModelContextProtocol.McpException>(() => _validateTmdlTool.ValidateAsync(request));
            Assert.Contains("At least one TMDL file must be provided", exception.Message);
            Assert.Equal(ModelContextProtocol.McpErrorCode.InvalidParams, exception.ErrorCode);
        }        

        [Fact]
        public async Task ValidateAsync_ShouldThrowMcpException_WhenInputIsNull()
        {
            // Act & Assert
            var request = new ValidateTmdlRequest { TmdlFiles = null };
            var exception = await Assert.ThrowsAsync<ModelContextProtocol.McpException>(() => _validateTmdlTool.ValidateAsync(request));
            Assert.Contains("TMDL files must be provided", exception.Message);
            Assert.Equal(ModelContextProtocol.McpErrorCode.InvalidParams, exception.ErrorCode);
        }

        [Fact]
        public async Task ValidateAsync_ShouldInvokeRunnerExactlyOnce()
        {
            // Arrange
            var files = new Dictionary<string, string> { { "model.tmdl", "model X {}" } };
            _testRunner.ShouldSucceed = true;
            _testRunner.CallCount = 0;

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = files };
            await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.Equal(1, _testRunner.CallCount);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllErrors_WhenRunnerReturnsMultipleErrors()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string> { { "model.tmdl", "bad" } };
            _testRunner.ShouldSucceed = false;
            _testRunner.ErrorMessages = new List<ValidationError> { 
                new ValidationError { Message = "Error 1" },
                new ValidationError { Message = "Error 2" },
                new ValidationError { Message = "Error 3" }
            };

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var result = await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal(3, result.Errors.Count);
            Assert.Equal("Error 1", result.Errors[0].Message);
            Assert.Equal("Error 2", result.Errors[1].Message);
            Assert.Equal("Error 3", result.Errors[2].Message);
        }

        [Fact]
        public async Task ValidateAsync_ShouldHandleLargeInputSet()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>();
            for (int i = 0; i < 1000; i++)
                tmdlFiles[$"file{i}.tmdl"] = $"model Model{i} {{ }}";
            _testRunner.ShouldSucceed = true;
            _testRunner.ErrorMessages = new List<ValidationError>();

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var result = await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal(1000, _testRunner.LastFilesValidated.Count);
        }

        [Fact]
        public async Task ValidateAsync_ShouldPreserveFilePathsAndCase()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>
            {
                { "Model.TMDL", "model X {}" },
                { "tables/Deep/Nested/Table1.tmdl", "table T {}" },
                { "TABLES/table2.TMDL", "table T2 {}" }
            };
            _testRunner.ShouldSucceed = true;
            _testRunner.ErrorMessages = new List<ValidationError>();

            // Act
            var request = new ValidateTmdlRequest { TmdlFiles = tmdlFiles };
            var result = await _validateTmdlTool.ValidateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.Equal(3, _testRunner.LastFilesValidated.Count);
            Assert.Contains("Model.TMDL", _testRunner.LastFilesValidated.Keys);
            Assert.Contains("tables/Deep/Nested/Table1.tmdl", _testRunner.LastFilesValidated.Keys);
            Assert.Contains("TABLES/table2.TMDL", _testRunner.LastFilesValidated.Keys);
        }
    }
}
