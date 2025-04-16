using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IPopUpsService
    {
        Task<string> RunPopupAnalysisAsync(string url, Dictionary<string, object> scrapedData);
        Task<string> AnalyzePopupsAsync(string htmlContent, List<string> externalJsContents);
    }
}