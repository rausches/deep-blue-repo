using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService)
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
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
                    "Custom" => await RunCustomAnalysisAsync(url, category.Name, scrapedData),
                    _ => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                };

                if (!string.IsNullOrEmpty(message))
                {
                    var designIssue = new DesignIssue
                    {
                        CategoryId = category.Id,
                        ReportId = report.Id,
                        Message = message,
                        Severity = 1 // Optionally, calculate dynamic severity
                    };

                    _dbContext.DesignIssues.Add(designIssue);
                    scanResults.Add(designIssue);
                }
            }

            await _dbContext.SaveChangesAsync();
            return scanResults;
        }

        private Task<string> RunCustomAnalysisAsync(string url, string categoryName, Dictionary<string, object> scrapedData)
        {
            // Implement your custom analysis logic here.
            return Task.FromResult(string.Empty);
        }
    }
}
