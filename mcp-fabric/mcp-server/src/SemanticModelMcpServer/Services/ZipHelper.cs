using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SemanticModelMcpServer.Services
{
    public static class ZipHelper
    {
        public static byte[] PackTmdl(Dictionary<string, string> files)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files), "Files collection cannot be null");
                
            if (files.Count == 0)
                throw new ArgumentException("Files collection cannot be empty", nameof(files));
                
            using var ms = new MemoryStream();
            using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var (path, text) in files)
                {
                    var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
                    using var sw = new StreamWriter(entry.Open());
                    sw.Write(text);
                }
            }
            
            // Important: reset position to beginning of stream before returning
            return ms.ToArray();
        }
    }
}