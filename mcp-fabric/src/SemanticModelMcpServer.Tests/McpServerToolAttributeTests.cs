using System;
using System.Linq;
using System.Reflection;
using ModelContextProtocol;
using Xunit;
using SemanticModelMcpServer.Tools;

namespace SemanticModelMcpServer.Tests
{
    public class McpServerToolAttributeTests
    {
        [Fact]
        public void AllToolClasses_HaveMcpServerToolTypeAttribute()
        {
            // Arrange
            var toolTypes = new[]
            {
                typeof(CreateSemanticModelTool),
                typeof(UpdateSemanticModelTool),
                typeof(RefreshTool),
                typeof(DeploymentTool),
                typeof(ValidateTmdlTool)
            };

            // Act & Assert
            foreach (var type in toolTypes)
            {
                var hasAttribute = type.GetCustomAttributes(typeof(McpServerToolTypeAttribute), inherit: false).Any();
                Assert.True(hasAttribute, $"{type.Name} is missing [McpServerToolType] attribute");
            }
        }

        [Fact]
        public void AllToolMethods_HaveMcpServerToolAttribute()
        {
            // Arrange
            var toolTypes = new[]
            {
                typeof(CreateSemanticModelTool),
                typeof(UpdateSemanticModelTool),
                typeof(RefreshTool),
                typeof(DeploymentTool),
                typeof(ValidateTmdlTool)
            };

            // Act & Assert
            foreach (var type in toolTypes)
            {
                var method = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.GetCustomAttributes(typeof(McpServerToolAttribute), false).Any());
                Assert.NotNull(method);
            }
        }
    }
}
