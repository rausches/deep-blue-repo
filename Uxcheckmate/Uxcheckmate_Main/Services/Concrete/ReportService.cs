using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class ReportService : IReportService
    {
        private readonly HttpClient _httpClient; 
        private readonly ILogger<ReportService> _logger; 
        private readonly UxCheckmateDbContext _dbContext;
        private readonly IOpenAiService _openAiService; 
        private readonly IBrokenLinksService _brokenLinksService;

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService)
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
        }

        public async Task<List<DesignIssue>> GenerateReportAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL cannot be empty.", nameof(url));
            }

            var scraper = new WebScraperService(_httpClient);
            var scrapedData = await scraper.ScrapeAsync(url);

            var report = new Report
            {
                Url = url,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
            };

            _dbContext.Reports.Add(report);
            await _dbContext.SaveChangesAsync();

            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            var scanResults = new List<DesignIssue>();

            foreach (var category in designCategories)
            {
                string message = category.ScanMethod switch
                {
                    "OpenAI" => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                    "Custom" => await RunCustomAnalysisAsync(url, category.Name, category.Description, scrapedData),
                    _ => "",
                };

                if (!string.IsNullOrEmpty(message))
                {
                    var designIssue = new DesignIssue
                    {
                        CategoryId = category.Id,
                        ReportId = report.Id,
                        Message = message,
                        Severity = DetermineSeverity(message)
                    };

                    _dbContext.DesignIssues.Add(designIssue);
                    scanResults.Add(designIssue);
                }
            }

            await _dbContext.SaveChangesAsync();
            return scanResults;
        }

        private async Task<string> RunCustomAnalysisAsync(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData)
        {
            switch (categoryName)
            {
                case "Broken Links":
                    return await _brokenLinksService.BrokenLinkAnalysis(url, scrapedData);
                // Add additional cases for other custom analyses here
                default:
                    return string.Empty;
            }
        }

        private int DetermineSeverity(string aiText)
        {
            if (aiText.Contains("critical", StringComparison.OrdinalIgnoreCase)) return 3; // High Severity
            if (aiText.Contains("should", StringComparison.OrdinalIgnoreCase)) return 2; // Medium Severity
            return 1; // Low Severity
        }
    }
}
