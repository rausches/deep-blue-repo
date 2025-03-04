using System.Net.Http;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public class HeadingHierarchyService : IHeadingHierarchyService
    {
        public async Task<string> AnalyzeAsync(string url)
        {
            var headerChecker = await Models.CheckHtmlHierarchy.CreateFromUrlAsync(url);
            if (headerChecker.problemsFound()){
                return string.Join("\n", headerChecker.ProblemSpots());
            }
            return string.Empty;
        }
    }
}