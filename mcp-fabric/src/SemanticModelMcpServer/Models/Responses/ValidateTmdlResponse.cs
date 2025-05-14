using System.Collections.Generic;
using SemanticModelMcpServer.Services;

namespace SemanticModelMcpServer.Models.Responses
{
    public class ValidateTmdlResponse
    {
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public string Summary { get; set; }

        public static ValidateTmdlResponse FromValidationResult(ValidationResult result)
        {
            return new ValidateTmdlResponse
            {
                IsValid = result.IsValid,
                Errors = result.Errors,
                Summary = result.Summary
            };
        }
    }
}
