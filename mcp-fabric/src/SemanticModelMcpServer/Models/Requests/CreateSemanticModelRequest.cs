using System.Collections.Generic;

namespace SemanticModelMcpServer.Models.Requests
{
    public class CreateSemanticModelRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> TmdlFiles { get; set; }

        public CreateSemanticModelRequest()
        {
            TmdlFiles = new Dictionary<string, string>();
        }
    }
}