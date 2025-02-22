using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IOpenAiService

    { 
        public Task<List<DesignIssue>> AnalyzeWebsite(string url);
        public Task<List<DesignIssue>> AnalyzeAndSaveDesignIssues(string url);
    }
}