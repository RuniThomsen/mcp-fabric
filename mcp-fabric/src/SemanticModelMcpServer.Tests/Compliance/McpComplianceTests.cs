// filepath: d:\repos\mcp-fabric\src\SemanticModelMcpServer.Tests\Compliance\McpComplianceTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Xunit;

namespace SemanticModelMcpServer.Tests.Compliance
{
    /// <summary>
    /// Base class for MCP compliance tests
    /// </summary>
    public class McpComplianceTests
    {
        [Fact]
        public void AllTests_ShouldBeInCorrectNamespace()
        {
            // Verify that all compliance tests are in the correct namespace
            var assembly = typeof(McpComplianceTests).Assembly;
            var testTypes = assembly.GetTypes();
            
            foreach (var type in testTypes)
            {
                if (type.Name.EndsWith("Tests") && type.Namespace == "SemanticModelMcpServer.Tests.Compliance")
                {
                    // Verify that these types actually contain test methods
                    var testMethods = type.GetMethods().Where(m => m.GetCustomAttributes(typeof(FactAttribute), true).Length > 0);
                    Assert.True(testMethods.Any(), $"Type {type.Name} should contain test methods");
                }
            }
        }        [Fact]
        public void AllTools_ShouldHaveComplianceTests()
        {
            // Get all tool types from our server
            var toolAssembly = typeof(SemanticModelMcpServer.Tools.CreateSemanticModelTool).Assembly;
            var toolTypes = toolAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Tool") && !t.IsInterface && !t.IsAbstract)
                .ToList();

            // Make sure we have at least one tool implementation
            Assert.True(toolTypes.Count() > 0, "Should have at least one tool implementation");
            
            // Verify that each tool has a corresponding test in one of our test files
            // This is a simplistic check - in reality we'd use reflection to examine each test file
            var assembly = typeof(McpComplianceTests).Assembly;
            var testFile = System.IO.File.ReadAllText(assembly.Location.Replace(".dll", ".dll"));
            
            foreach (var tool in toolTypes)
            {
                Assert.Contains(tool.Name, testFile, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}