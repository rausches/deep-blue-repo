using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IOpenAiService

    { 
        public Task<string> GetChatResponse(string message);

        public Task<string> AnalyzeUx(string url);
    }
}