using System.Collections.Generic;

namespace SemanticModelMcpServer.Models.Requests
{
    public class UpdateSemanticModelRequest
    {
        public string ModelId { get; set; }
        public Dictionary<string, string> TmdlFiles { get; set; }
    }
}