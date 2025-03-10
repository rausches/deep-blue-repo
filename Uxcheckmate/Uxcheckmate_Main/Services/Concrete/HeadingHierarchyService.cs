using System.Net.Http;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public class HeadingHierarchyService : IHeadingHierarchyService
    {
        public Task<string> AnalyzeAsync(Dictionary<string, object> scrapedData)
        {
            if (!scrapedData.ContainsKey("htmlContent")){
                Console.WriteLine("[DEBUG] No 'htmlContent' in scraped data");
                return Task.FromResult("[Error] No HTML content available.");
            }
            string htmlContent = (string)scrapedData["htmlContent"];
            var headerChecker = Models.CheckHtmlHierarchy.CreateFromHtml(htmlContent);
            if (headerChecker.problemsFound()){
                return Task.FromResult(string.Join("\n", headerChecker.ProblemSpots()));
            }
            return Task.FromResult(string.Empty);
        }
    }
}