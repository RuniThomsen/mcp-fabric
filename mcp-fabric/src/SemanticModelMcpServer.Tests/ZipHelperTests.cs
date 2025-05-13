using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Tests
{
    public class ZipHelperTests
    {
        [Fact]
        public void PackTmdl_ShouldCreateZipArchive_WhenFilesAreProvided()
        {
            // Arrange
            var files = new Dictionary<string, string>
            {
                { "file1.tmdl", "Content of file 1" },
                { "file2.tmdl", "Content of file 2" }
            };

            // Act
            var result = ZipHelper.PackTmdl(files);

            // Assert
            using (var memStream = new MemoryStream(result))
            {
                using (var archive = new ZipArchive(memStream, ZipArchiveMode.Read))
                {
                    Assert.Equal(2, archive.Entries.Count);
                    Assert.Contains(archive.Entries, e => e.FullName == "file1.tmdl");
                    Assert.Contains(archive.Entries, e => e.FullName == "file2.tmdl");
                    
                    // Verify that we can read the contents
                    foreach (var entry in archive.Entries)
                    {
                        using (var reader = new StreamReader(entry.Open()))
                        {
                            string content = reader.ReadToEnd();
                            Assert.NotNull(content);
                            Assert.Equal(entry.FullName == "file1.tmdl" ? "Content of file 1" : "Content of file 2", content);
                        }
                    }
                }
            }
        }

        [Fact]
        public void PackTmdl_ShouldThrowArgumentNullException_WhenFilesAreNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ZipHelper.PackTmdl(null));
        }

        [Fact]
        public void PackTmdl_ShouldThrowArgumentException_WhenFilesAreEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ZipHelper.PackTmdl(new Dictionary<string, string>()));
        }
    }
}