using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IBrokenLinksService
    {
        Task<string> BrokenLinkAnalysis(string Url, Dictionary<string, object> scrapedData);
        Task<List<string>> CheckBrokenLinksAsync(List<string> links);
    }
}