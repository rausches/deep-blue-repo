using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IAudioService
    {
        Task<string> RunAudioAnalysisAsync(string url, Dictionary<string, object> scrapedData);
    }
}
