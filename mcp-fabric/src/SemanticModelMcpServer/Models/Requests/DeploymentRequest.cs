namespace SemanticModelMcpServer.Models.Requests
{
    public class DeploymentRequest
    {
        public string ModelId { get; set; }
        public string TargetEnvironment { get; set; }
    }
}
