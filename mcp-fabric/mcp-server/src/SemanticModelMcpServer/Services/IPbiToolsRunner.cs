using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticModelMcpServer.Services
{
    public interface IPbiToolsRunner
    {
        Task<ValidationResult> ValidateAsync(Dictionary<string, string> tmdlFiles);
    }
}
