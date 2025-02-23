using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IBrokenLinksService
    {
        public Task<string> BrokenLinkAnalysis(string Url, Dictionary<string, object> scrapedData);
    }
}