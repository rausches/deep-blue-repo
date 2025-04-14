using System.Collections.Generic;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public interface IScrollService
    {
        Task<string> RunScrollAnalysisAsync(string url, Dictionary<string, object> scrapedData);
    }
}
