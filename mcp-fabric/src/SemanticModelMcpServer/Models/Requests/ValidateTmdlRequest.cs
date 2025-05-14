using System.Collections.Generic;

namespace SemanticModelMcpServer.Models.Requests
{
    public class ValidateTmdlRequest
    {
        public Dictionary<string, string> TmdlFiles { get; set; } = new Dictionary<string, string>();
    }
}
