namespace SemanticModelMcpServer.Models.Requests
{
    public class RefreshSemanticModelRequest
    {
        public string ModelId { get; set; }
        public string Type { get; set; } = "Full"; // Default to Full refresh
    }
}