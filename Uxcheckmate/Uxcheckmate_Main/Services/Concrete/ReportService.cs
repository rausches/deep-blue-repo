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
        private readonly IBrokenLinksService _brokenLinksService;
        private readonly IHeadingHierarchyService _headingHierarchyService;
        private readonly IColorSchemeService _colorSchemeService;
        private readonly IDynamicSizingService _dynamicSizingService;
        private readonly IFaviconDetectionService _faviconDetectionService;

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService, IColorSchemeService colorSchemeService, IDynamicSizingService dynamicSizingService, IFaviconDetectionService faviconDetectionService)

        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
            _headingHierarchyService = headingHierarchyService;
            _colorSchemeService = colorSchemeService;
            _dynamicSizingService = dynamicSizingService;
            _faviconDetectionService = faviconDetectionService; // Assigned here
        }

        public async Task<ICollection<DesignIssue>> GenerateReportAsync(Report report)
        {
            var url = report.Url;
            _logger.LogInformation("Starting report generation for URL: {Url}", url);

            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("URL is null or empty.");
                throw new ArgumentException("URL cannot be empty.", nameof(url));
            }

            var scraper = new WebScraperService(_httpClient);
            _logger.LogDebug("Scraping content from URL: {Url}", url);
            var scrapedData = await scraper.ScrapeAsync(url);
            _logger.LogDebug("Scraping completed for URL: {Url}", url);

            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            _logger.LogInformation("Found {Count} design categories.", designCategories.Count);

            var scanResults = new List<DesignIssue>();

            foreach (var category in designCategories)
            {
                _logger.LogInformation("Analyzing category: {CategoryName} using scan method: {ScanMethod}", category.Name, category.ScanMethod);

                string message = category.ScanMethod switch
                {
                    "OpenAI" => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                    "Custom" => await RunCustomAnalysisAsync(url, category.Name, category.Description, scrapedData),
                    _ => ""
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
                    _logger.LogInformation("Design issue added for category {CategoryName} with severity {Severity}.", category.Name, designIssue.Severity);
                }
                else
                {
                    _logger.LogInformation("No issues found for category: {CategoryName}", category.Name);
                }
            }

            await _dbContext.SaveChangesAsync();
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
                case "Color Scheme":
                    _logger.LogDebug("Delegating Color Scheme analysis for URL: {Url}", url);
                    return await _colorSchemeService.AnalyzeWebsiteColorsAsync(url);
                case "Mobile Responsiveness":
                    _logger.LogDebug("Delegating Dynamic Sizing analysis for URL: {Url}", url);
                    var hasDynamicSizing = _dynamicSizingService.HasDynamicSizing(scrapedData["htmlContent"].ToString());
                    

                    if (!hasDynamicSizing)
                    {
                        string prompt = $"The website at {url} is missing dynamic sizing elements (e.g., media queries, viewport meta tag, flexbox/grid layout). Please provide a recommendation on how to implement dynamic sizing.";
                        return await _openAiService.AnalyzeWithOpenAI(url, "Dynamic Sizing", "Check if the website has proper dynamic sizing elements", scrapedData);
                    }
                    else
                    {
                        _logger.LogDebug("Dynamic sizing elements are present. No recommendations needed.");
                        return string.Empty;
                    }

                case "Favicon Check":
                    _logger.LogDebug("Checking for favicon on URL: {Url}", url);
                    var (hasFavicon, faviconUrl) = await _faviconDetectionService.DetectFaviconAsync(url);

                    if (!hasFavicon)
                    {
                        _logger.LogWarning("No favicon detected for URL: {Url}", url);
                        return "No favicon found on this website. Consider adding a favicon for better branding and user recognition.";
                    }
                    else
                    {
                        _logger.LogInformation("Favicon found: {FaviconUrl}", faviconUrl);
                        return string.Empty;
                    }

                default:
                    _logger.LogDebug("No custom analysis implemented for category: {CategoryName}", categoryName);
                    return string.Empty;
            }
        }

        private int DetermineSeverity(string aiText)
        {
            _logger.LogDebug("Determining severity for analysis text.");
            return aiText.Contains("critical", StringComparison.OrdinalIgnoreCase) ? 3 :
                   aiText.Contains("should", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        }
    }
}