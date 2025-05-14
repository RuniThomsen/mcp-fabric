using System.Collections.Generic;

namespace SemanticModelMcpServer.Services
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public string Summary { get; set; }
    }

    public class ValidationError
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public int Column { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string Severity { get; set; } = "Error";
    }
}
