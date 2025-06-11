using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Uxcheckmate_Main.Services
{

    public interface IPlaywrightApiService
    {
        Task<PlaywrightAnalysisResult?> AnalyzeWebsiteAsync(string url, bool fullPage = false);
    }
}