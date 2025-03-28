using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace Uxcheckmate_Main.Services
{
    public interface IWebScraperService
    {
        Task<string> FetchHtmlAsync(string url);

        Dictionary<string, object> ExtractHtmlElements(string htmlContent, string url);


        Task<Dictionary<string, object>> ScrapeAsync(string url);
    }
}
