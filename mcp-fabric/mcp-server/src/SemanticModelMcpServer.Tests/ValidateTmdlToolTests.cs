using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using SemanticModelMcpServer.Services;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer.Tests
{
    // Test implementation of IPbiToolsRunner for testing
    public class TestPbiToolsRunner : IPbiToolsRunner
    {
        public bool ShouldSucceed { get; set; } = true;
        public List<string> ErrorMessages { get; set; } = new List<string>();
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
                    Errors = new List<string> { "No TMDL files provided for validation" }
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

        public ValidateTmdlToolTests()
        {
            _testRunner = new TestPbiToolsRunner();
            _validateTmdlTool = new ValidateTmdlTool(_testRunner);
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
            _testRunner.ErrorMessages = new List<string>();

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

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
            _testRunner.ErrorMessages = new List<string> { "Error: Syntax error at line 1: Missing closing brace" };

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Single(result.Errors);
            Assert.Contains("Syntax error", result.Errors[0]);
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
            var exception = await Assert.ThrowsAsync<Exception>(() => _validateTmdlTool.ValidateAsync(tmdlFiles));
            Assert.Contains("PBI Tools", exception.Message);
        }

        [Fact]
        public async Task ValidateAsync_ShouldHandleEmptyTmdlFiles()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>();

            _testRunner.ShouldSucceed = false;
            _testRunner.ErrorMessages = new List<string> { "No TMDL files provided for validation" };

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Contains("No TMDL files provided", result.Errors[0]);
        }

        [Fact]
        public async Task ValidateAsync_ShouldThrowArgumentNullException_WhenInputIsNull()
        {
            // Act
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => _validateTmdlTool.ValidateAsync(null));
            Assert.Contains("tmdlFiles", ex.ParamName);
        }

        [Fact]
        public async Task ValidateAsync_ShouldInvokeRunnerExactlyOnce()
        {
            // Arrange
            var files = new Dictionary<string, string> { { "model.tmdl", "model X {}" } };
            _testRunner.ShouldSucceed = true;
            _testRunner.CallCount = 0;

            // Act
            await _validateTmdlTool.ValidateAsync(files);

            // Assert
            Assert.Equal(1, _testRunner.CallCount);
        }

        [Fact]
        public async Task ValidateAsync_ShouldReturnAllErrors_WhenRunnerReturnsMultipleErrors()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string> { { "model.tmdl", "bad" } };
            _testRunner.ShouldSucceed = false;
            _testRunner.ErrorMessages = new List<string> { "Error 1", "Error 2", "Error 3" };

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.Equal(3, result.Errors.Count);
            Assert.Contains("Error 1", result.Errors);
            Assert.Contains("Error 2", result.Errors);
            Assert.Contains("Error 3", result.Errors);
        }

        [Fact]
        public async Task ValidateAsync_ShouldHandleLargeInputSet()
        {
            // Arrange
            var tmdlFiles = new Dictionary<string, string>();
            for (int i = 0; i < 1000; i++)
                tmdlFiles[$"file{i}.tmdl"] = $"model Model{i} {{ }}";
            _testRunner.ShouldSucceed = true;
            _testRunner.ErrorMessages = new List<string>();

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

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
            _testRunner.ErrorMessages = new List<string>();

            // Act
            var result = await _validateTmdlTool.ValidateAsync(tmdlFiles);

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
