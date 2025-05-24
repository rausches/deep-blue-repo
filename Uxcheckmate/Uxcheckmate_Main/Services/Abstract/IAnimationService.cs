using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IAnimationService
    {
        Task<string> AnalyzeAnimationsAsync(string htmlContent, List<string> inlineCss, List<string> externalCss, List<string> inlineJs, List<string> externalJs);
        Task<string> RunAnimationAnalysisAsync(string url, Dictionary<string, object> scrapedData);
    }
}