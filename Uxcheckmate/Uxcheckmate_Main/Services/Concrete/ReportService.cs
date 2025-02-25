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
        private readonly IHeadingHierarchyService _headingHierarchyService;

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService)
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
            _headingHierarchyService = headingHierarchyService;
        }

        public async Task<ICollection<DesignIssue>> GenerateReportAsync(Report report)
        {
            var url = report.Url;
            _logger.LogInformation("Starting report generation for URL: {Url}", url);
            // Validate the URL input.
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("URL is null or empty.");
                throw new ArgumentException("URL cannot be empty.", nameof(url));
            }

            // Scrape the website content.
            var scraper = new WebScraperService(_httpClient);
            _logger.LogDebug("Scraping content from URL: {Url}", url);
            var scrapedData = await scraper.ScrapeAsync(url);
            _logger.LogDebug("Scraping completed for URL: {Url}", url);

            // Retrieve design categories from the database.
            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            _logger.LogInformation("Found {Count} design categories.", designCategories.Count);

            var scanResults = new List<DesignIssue>();

            // Analyze each design category.
            foreach (var category in designCategories)
            {
                _logger.LogInformation("Analyzing category: {CategoryName} using scan method: {ScanMethod}", category.Name, category.ScanMethod);
                string message = category.ScanMethod switch
                {
                    "OpenAI" => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                    "Custom" => await RunCustomAnalysisAsync(url, category.Name, category.Description, scrapedData),
                    _ => ""
                };

                // If analysis returns a message, record it as a design issue.
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
                    _logger.LogInformation("Design issue added for category {CategoryName} with severity {Severity}.", category.Name, designIssue.Severity);
                }
                else
                {
                    _logger.LogInformation("No issues found for category: {CategoryName}", category.Name);
                }
            }

            // Save all design issues to the database.
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Report generation completed for URL: {Url}. Total issues found: {IssueCount}", url, scanResults.Count);

            return scanResults;
        }

        private async Task<string> RunCustomAnalysisAsync(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData)
        {
            _logger.LogInformation("Running custom analysis for category: {CategoryName}", categoryName);
            switch (categoryName)
            {
                case "Broken Links":
                    _logger.LogDebug("Delegating Broken Links analysis for URL: {Url}", url);
                    return await _brokenLinksService.BrokenLinkAnalysis(url, scrapedData);
                case "Visual Hierarchy":
                    _logger.LogDebug("Delegating Visual Hierarchy analysis for URL: {Url}", url);
                    return await _headingHierarchyService.AnalyzeAsync(url);
                // Add additional cases for other custom analyses here
                default:
                    _logger.LogDebug("No custom analysis implemented for category: {CategoryName}", categoryName);
                    return string.Empty;
            }
        }

        private int DetermineSeverity(string aiText)
        {
            _logger.LogDebug("Determining severity for analysis text.");
            if (aiText.Contains("critical", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Severity determined as High.");
                return 3; // High Severity
            }
            if (aiText.Contains("should", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Severity determined as Medium.");
                return 2; // Medium Severity
            }
            _logger.LogDebug("Severity determined as Low.");
            return 1; // Low Severity
        }
    }
}
