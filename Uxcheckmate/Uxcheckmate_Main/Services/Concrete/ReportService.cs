using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

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
        private readonly IDynamicSizingService _dynamicSizingService;

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService, IDynamicSizingService dynamicSizingService)
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
            _headingHierarchyService = headingHierarchyService;
            _dynamicSizingService = dynamicSizingService;
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
            _logger.LogInformation("Generating PDF immediately after scraping...");
            await GenerateReportPdfAsync(report);

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
                case "Mobile Responsiveness":
                    _logger.LogDebug("Delegating Dynamic Sizing analysis for URL: {Url}", url);
                    var hasDynamicSizing = _dynamicSizingService.HasDynamicSizing(scrapedData["htmlContent"].ToString());

                if (!hasDynamicSizing)
                {
                    string prompt = $"The website at {url} is missing dynamic sizing elements (e.g., media queries, viewport meta tag, flexbox/grid layout). Please provide a recommendation on how to implement dynamic sizing.";
                    return await _openAiService.AnalyzeWithOpenAI(url, "Dynamic Sizing", "Check if the website has proper dynamic sizing elemenst", scrapedData);

                }
                else
                {
                    _logger.LogDebug("Dynamic sizing elements are present. No recommendations needed.");
                    return string.Empty;
                }

                default:
                    _logger.LogDebug("No custom analysis implemented for category: {CategoryName}", categoryName);
                    return string.Empty;
            }
                // Add additional cases for other custom analyses here

        }

        private int DetermineSeverity(string aiText)
        {
            _logger.LogDebug("Determining severity for analysis text.");
            return aiText.Contains("critical", StringComparison.OrdinalIgnoreCase) ? 3 :
                   aiText.Contains("should", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        }

        public async Task<byte[]> GenerateReportPdfAsync(Report report)
        {
            var issues = await _dbContext.DesignIssues.Where(i => i.ReportId == report.Id).ToListAsync();
            return await GeneratePdfFromIssues(report, issues);
        }

        private async Task<byte[]> GeneratePdfFromIssues(Report report, ICollection<DesignIssue> issues)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            document.Add(new Paragraph($"UX Report for {report.Url}")
                .SetFont(fontBold)
                .SetFontSize(16));

            document.Add(new Paragraph($"Generated on: {DateTime.UtcNow}")
                .SetFont(fontRegular)
                .SetFontSize(10));

            document.Add(new Paragraph("\n--- UX Issues Found ---\n")
                .SetFont(fontBold)
                .SetFontSize(14));

            if (issues == null || issues.Count == 0)
            {
                document.Add(new Paragraph("No issues found.")
                    .SetFont(fontRegular)
                    .SetFontSize(12));
            }
            else
            {
                foreach (var issue in issues)
                {
                    document.Add(new Paragraph($"Category: {issue.CategoryId}")
                        .SetFont(fontBold)
                        .SetFontSize(12));

                    document.Add(new Paragraph($"Severity: {issue.Severity}")
                        .SetFont(fontRegular)
                        .SetFontSize(11));

                    document.Add(new Paragraph(issue.Message)
                        .SetFont(fontRegular)
                        .SetFontSize(10));

                    document.Add(new Paragraph("\n"));
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
}
