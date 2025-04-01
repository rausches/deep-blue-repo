using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
        private readonly IWebScraperService _scraperService;
        private readonly IScreenshotService _screenshotService;
        // private readonly IFaviconDetectionService _faviconDetectionService; // Commented out

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService, IColorSchemeService colorSchemeService, IDynamicSizingService dynamicSizingService, IScreenshotService screenshotService, IWebScraperService scraperService) /*, IFaviconDetectionService faviconDetectionService*/
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
            _headingHierarchyService = headingHierarchyService;
            _colorSchemeService = colorSchemeService;
            _dynamicSizingService = dynamicSizingService;
            _screenshotService = screenshotService;
            _scraperService = scraperService;
            // _faviconDetectionService = faviconDetectionService; // Commented out
        }

        public async Task<ICollection<DesignIssue>> GenerateReportAsync(Report report)
        {
            // Initialize url to report attribute
            var url = report.Url;
            _logger.LogInformation("Starting report generation for URL: {Url}", url);

            // If there is no url throw an exception
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogError("URL is null or empty.");
                throw new ArgumentException("URL cannot be empty.", nameof(url));
            }

            // Call the scraper
            var scrapedData = await _scraperService.ScrapeAsync(url);
            _logger.LogDebug("Scraping completed for URL: {Url}", url);

            // Get list of design categories
            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            _logger.LogInformation("Found {Count} design categories.", designCategories.Count);

            // Take screenshot once to be used by multiple analyzers
            Task<byte[]> screenshotTask = _screenshotService?.CaptureFullPageScreenshot(url) ?? Task.FromResult(new byte[0]);

            // Instantate Design Issue list
            var scanResults = new ConcurrentBag<DesignIssue>();

            // Run analysis for each category in parallel
            await Parallel.ForEachAsync(
                designCategories,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                async (category, cancellationToken) =>
                {
                    _logger.LogInformation("Analyzing category: {CategoryName} using scan method: {ScanMethod}", category.Name, category.ScanMethod);

                    string message;
                    try
                    {
                        message = category.ScanMethod switch
                        {
                            "OpenAI" => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                            "Custom" => await RunCustomAnalysisAsync(url, category.Name, category.Description, scrapedData, screenshotTask),
                            _ => ""
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error analyzing category {CategoryName}: {ErrorMessage}", category.Name, ex.Message);
                        message = "";
                    }

                    // Connect service response to dbset attributes
                    if (!string.IsNullOrEmpty(message))
                    {
                        var designIssue = new DesignIssue
                        {
                            CategoryId = category.Id,
                            ReportId = report.Id,
                            Message = message,
                            Severity = DetermineSeverity(message)
                        };

                        // Add to results collection
                        scanResults.Add(designIssue);
                        _logger.LogInformation("Design issue added for category {CategoryName} with severity {Severity}.", category.Name, designIssue.Severity);
                    }
                    else
                    {
                        _logger.LogInformation("No issues found for category: {CategoryName}", category.Name);
                    }
                });

            // Add all design issues to the context
            _dbContext.DesignIssues.AddRange(scanResults);

            // Save to database
            await _dbContext.SaveChangesAsync();

            // Return report
            return scanResults.ToList();
        }

        public async Task<string> RunCustomAnalysisAsync(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData, Task<byte[]> screenshotTask = null)
        {
            _logger.LogInformation("Running custom analysis for category: {CategoryName}", categoryName);
            // If no screenshot was provided, capture it now
            if (screenshotTask == null)
            {
                screenshotTask = _screenshotService?.CaptureFullPageScreenshot(url) ?? Task.FromResult(new byte[0]);
            }

            switch (categoryName)
            {
                case "Broken Links":
                    _logger.LogDebug("Delegating Broken Links analysis for URL: {Url}", url);
                    return await _brokenLinksService.BrokenLinkAnalysis(url, scrapedData);

                case "Visual Hierarchy":
                    _logger.LogDebug("Delegating Visual Hierarchy analysis for URL: {Url}", url);
                    return await _headingHierarchyService.AnalyzeAsync(scrapedData);
                case "Color Scheme":
                    _logger.LogDebug("Delegating Color Scheme analysis for URL: {Url}", url);
                    return await _colorSchemeService.AnalyzeWebsiteColorsAsync(scrapedData, screenshotTask);

                case "Mobile Responsiveness":
                    _logger.LogDebug("Delegating Dynamic Sizing analysis for URL: {Url}", url);
                    var hasDynamicSizing = _dynamicSizingService.HasDynamicSizing(scrapedData["htmlContent"].ToString());
                // Add your custom analysis here:
                // Example: case "Category Name":
                    // return await _servicename.method(url, scrapeddata)
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

                case "Favicon":
                    _logger.LogDebug("Checking for favicon on URL: {Url}", url);
                    bool hasFavicon = scrapedData.ContainsKey("hasFavicon") && scrapedData["hasFavicon"] is bool value && value;

                    if (!hasFavicon)
                    {
                        _logger.LogWarning("❌ No favicon detected for URL: {Url}", url);
                        return "No favicon found on this website. Consider adding a favicon for better branding and user recognition.";
                    }
                    else
                    {
                        _logger.LogInformation("✅ Favicon detected for URL: {Url}", url);
                        return string.Empty;
                    }
                case "Font Legibility":
                    if (scrapedData.TryGetValue("fonts", out var fontsObj) && fontsObj is List<string> fontsUsed)
                    {
                        _logger.LogInformation("Extracted fonts: {@Fonts}", fontsUsed); // Log extracted fonts

                        // Normalize both lists to lowercase for case-insensitive comparison
                        var illegibleFontsNormalized = FontLegibilityModel.IllegibleFonts
                            .Select(f => f.ToLowerInvariant().Trim())
                            .ToList();

                        var fontsUsedNormalized = fontsUsed
                            .Select(f => f.ToLowerInvariant().Trim().Trim('"'))
                            .ToList();

                        _logger.LogInformation("Normalized Illegible Fonts: {@IllegibleFonts}", illegibleFontsNormalized);
                        _logger.LogInformation("Normalized Extracted Fonts: {@FontsUsed}", fontsUsedNormalized);

                        // Debug log: Check EXACTLY what's in both lists
                        _logger.LogInformation("Comparing extracted fonts with illegible fonts...");

                        foreach (var font in fontsUsedNormalized)
                        {
                            if (illegibleFontsNormalized.Contains(font))
                            {
                                _logger.LogInformation("Match found: {Font} is illegible!", font);
                            }
                        }

                        var illegibleFontsFound = fontsUsedNormalized.Intersect(illegibleFontsNormalized).ToList();

                        if (illegibleFontsFound.Any())
                        {
                            string issueMessage = $"The following fonts are considered illegible: {string.Join(", ", illegibleFontsFound)}. Consider using more readable fonts for better accessibility.";
                            _logger.LogInformation("Issue detected: {IssueMessage}", issueMessage);
                            return issueMessage;
                        }
                        else
                        {
                            _logger.LogInformation("✅ No illegible fonts detected for URL: {Url}", url);
                            return string.Empty;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("❌ Font extraction failed or no fonts found for URL: {Url}", url);
                        return "No fonts were detected on this website. Ensure that text elements specify a font-family.";
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