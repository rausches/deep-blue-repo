using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Uxcheckmate_Main.Models;
public interface IPlaywrightScraperService
{
    Task<ScrapedContent> ScrapeAsync(string url);
}