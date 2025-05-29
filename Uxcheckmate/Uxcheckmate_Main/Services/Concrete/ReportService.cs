using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;


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
        private readonly IMobileResponsivenessService _mobileResponsivenessService;
        private readonly IScreenshotService _screenshotService;
        private readonly IPlaywrightScraperService _playwrightScraperService;
        private readonly IPopUpsService _popUpsService;
        private readonly IAnimationService _animationService;
        private readonly IAudioService _audioService;
        private readonly IScrollService _scrollService;
        private readonly IFPatternService _fPatternService;
        private readonly IZPatternService _zPatternService;
        private readonly ISymmetryService _symmetryService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService, IColorSchemeService colorSchemeService, IMobileResponsivenessService mobileResponsivenessService, IScreenshotService screenshotService, IPlaywrightScraperService playwrightScraperService, IPopUpsService popUpsService, IAnimationService animationService, IAudioService audioService, IScrollService scrollService, IFPatternService fPatternService, IZPatternService zPatternService, ISymmetryService symmetryService, IServiceScopeFactory scopeFactory, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _dbContext = context;
            _openAiService = openAiService;
            _logger = logger;
            _brokenLinksService = brokenLinksService;
            _headingHierarchyService = headingHierarchyService;
            _colorSchemeService = colorSchemeService;
            _mobileResponsivenessService = mobileResponsivenessService;
            _screenshotService = screenshotService;
            _playwrightScraperService = playwrightScraperService;
            _popUpsService = popUpsService;
            _animationService = animationService;
            _audioService = audioService;
            _scrollService = scrollService;
            _fPatternService = fPatternService;
            _zPatternService = zPatternService;
            _symmetryService = symmetryService;
            _scopeFactory = scopeFactory;
            _cache = cache;
        }

        public async Task<ICollection<DesignIssue>> GenerateReportAsync(Report report, CancellationToken cancellationToken)
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

            // Thread safe design issues
            var scanResults = new ConcurrentBag<DesignIssue>();

            // Scrape site with caching
            var (scrapedData, fullScraped) = await GetScrapedContentAsync(url, cancellationToken);
            if (scrapedData == null)
                return scanResults.ToList();

            // Screenshot with caching
            var screenshotTask = GetOrCaptureScreenshot(url);

            // Get list of design categories
            var designCategories = await GetDesignCategoriesAsync();
            if (designCategories == null)
                return scanResults.ToList();

            // Run analysis for each category in parallel
            await RunCategoryAnalysesAsync(report, designCategories, scrapedData, fullScraped, screenshotTask, scanResults, cancellationToken);

            // Get Report Summary
            await GenerateAndAssignSummaryAsync(report, fullScraped.HtmlContent, scanResults.ToList(), url, cancellationToken);

            // Mark report as complete
            await MarkReportAsCompletedAsync(report.Id, cancellationToken);

            return scanResults.ToList();
        }

        public async Task<string> RunCustomAnalysisAsync(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData, ScrapedContent fullScraped, Task<byte[]> screenshotTask)
        {
            _logger.LogInformation("Running custom analysis for category: {CategoryName}", categoryName);

            string message = categoryName switch
            {
                "Broken Links"          => await _brokenLinksService.BrokenLinkAnalysis(url, scrapedData),
                "Visual Hierarchy"      => await _headingHierarchyService.AnalyzeAsync(scrapedData),
                "Color Scheme"          => await _colorSchemeService.AnalyzeWebsiteColorsAsync(scrapedData, screenshotTask),
                "Mobile Responsiveness" => await _mobileResponsivenessService.RunMobileAnalysisAsync(url, scrapedData),
                "Favicon"               => await AnalyzeFaviconAsync(url, scrapedData),
                "Font Legibility"       => await AnalyzeFontLegibilityAsync(url, scrapedData),
                "Pop Ups"               => await _popUpsService.RunPopupAnalysisAsync(url, scrapedData),
                "Animations"            => await _animationService.RunAnimationAnalysisAsync(url, scrapedData),
                "Audio"                 => await _audioService.RunAudioAnalysisAsync(url, scrapedData),
                "Number of scrolls"     => await _scrollService.RunScrollAnalysisAsync(url, scrapedData),
                "F Pattern"             => await _fPatternService.AnalyzeFPatternAsync(fullScraped.ViewportWidth, fullScraped.ViewportHeight, fullScraped.LayoutElements),
                "Z Pattern"             => await _zPatternService.AnalyzeZPatternAsync(fullScraped.ViewportWidth, fullScraped.ViewportHeight, fullScraped.LayoutElements),
                "Symmetry"              => await _symmetryService.AnalyzeSymmetryAsync(fullScraped.ViewportWidth, fullScraped.ViewportHeight, fullScraped.LayoutElements),
                _ => ""
            };

            return message;
        }

        private async Task<string> AnalyzeFaviconAsync(string url, Dictionary<string, object> scrapedData)
        {
            bool hasFavicon = scrapedData.TryGetValue("hasFavicon", out var hasFaviconObj) && hasFaviconObj is bool value && value;

            if (!hasFavicon)
            {
                _logger.LogWarning("❌ No favicon detected for URL: {Url}", url);
                return "No favicon found on this website. Consider adding a favicon for better branding and user recognition.";
            }

            _logger.LogInformation("✅ Favicon detected for URL: {Url}", url);
            return string.Empty;
        }

        private async Task<string> AnalyzeFontLegibilityAsync(string url, Dictionary<string, object> scrapedData)
        {
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
        }

        private int DetermineSeverity(string aiText)
        {
            _logger.LogDebug("Determining severity for analysis text.");
            return aiText.Contains("critical", StringComparison.OrdinalIgnoreCase) ? 3 :
                   aiText.Contains("should", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        }
                
        private async Task<(Dictionary<string, object>?, ScrapedContent?)> GetScrapedContentAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                string cacheKey = $"scrapedcontent_{url.ToLowerInvariant()}";

                // Try to get from cache
                if (!_cache.TryGetValue(cacheKey, out ScrapedContent fullScraped))
                {
                    _logger.LogInformation("No cached scrape found for {Url}. Scraping now.", url);
                    fullScraped = await _playwrightScraperService.ScrapeEverythingAsync(url, cancellationToken);

                    // Cache it for 1 hour
                    _cache.Set(cacheKey, fullScraped, TimeSpan.FromHours(1));
                }
                else
                {
                    _logger.LogInformation("Using cached scrape for {Url}.", url);
                }
                return (fullScraped.ToDictionary(), fullScraped);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scraping cancelled.");

                // Return what we have, even if empty
                return (null, null); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scraping failed.");

                // Return what we have, even if empty
                return (null, null);
            }
        }

        private Task<byte[]> GetOrCaptureScreenshot(string url)
        {
            string cacheKey = $"fullpage_screenshot_{url.ToLowerInvariant()}";

            // Check cache for existing full page screenshot
            if (_cache.TryGetValue(cacheKey, out byte[] cachedScreenshot))
            {
                _logger.LogInformation("Using cached full page screenshot for {Url}.", url);
                return Task.FromResult(cachedScreenshot);
            }

            _logger.LogInformation("Capturing new full page screenshot for {Url}.", url);

            // Capture screenshot and cache it after capture completes
            var screenshotTask = _screenshotService?.CaptureFullPageScreenshot(url) ?? Task.FromResult(new byte[0]);
            return screenshotTask.ContinueWith(t =>
            {
                var result = t.Result;
                if (result != null && result.Length > 0)
                {
                    // Cache for 1 hour
                    _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
                    _logger.LogInformation("Full page screenshot cached for {Url}.", url);
                }
                return result;
            });
        }

        private async Task<List<DesignCategory>?> GetDesignCategoriesAsync()
        {
            try
            {
                var categories = await _dbContext.DesignCategories.ToListAsync();
                _logger.LogInformation("Found {Count} design categories.", categories.Count);
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve design categories.");
                return null;
            }
        }

        private async Task RunCategoryAnalysesAsync(Report report, List<DesignCategory> categories, Dictionary<string, object> scrapedData, ScrapedContent fullScraped, Task<byte[]> screenshotTask, ConcurrentBag<DesignIssue> results, CancellationToken cancellationToken)
        {
            // Run parallel analysis for each design category using a limited number of concurrent threads
            await Parallel.ForEachAsync(categories, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (category, token) =>
            {
                _logger.LogInformation("Analyzing category: {CategoryName} using scan method: {ScanMethod}", category.Name, category.ScanMethod);

                try
                {
                    // Create a scoped database context for thread-safe access
                    using var scope = _scopeFactory.CreateScope();
                    var scopedDbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();

                    // Determine which analysis method to run based on the scan method type
                    string message = category.ScanMethod switch
                    {
                        "OpenAI" => await _openAiService.AnalyzeWithOpenAI(report.Url, category.Name, category.Description, scrapedData),
                        "Custom" => await RunCustomAnalysisAsync(report.Url, category.Name, category.Description, scrapedData, fullScraped, screenshotTask),
                        _ => ""
                    };

                    // If a message was returned, create and save the design issue
                    if (!string.IsNullOrEmpty(message))
                    {
                        var designIssue = new DesignIssue
                        {
                            CategoryId = category.Id,
                            ReportId = report.Id,
                            Message = message,
                            Severity = DetermineSeverity(message)
                        };

                        scopedDbContext.DesignIssues.Add(designIssue);
                        await scopedDbContext.SaveChangesAsync(token);
                        results.Add(designIssue);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Analysis cancelled for category {CategoryName}", category.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing category {CategoryName}: {ErrorMessage}", category.Name, ex.Message);
                }
            });
        }

        private async Task GenerateAndAssignSummaryAsync(Report report, string htmlContent, List<DesignIssue> issues, string url, CancellationToken cancellationToken)
        {
            try
            {
                // Generate a summary using OpenAI based on the list of issues and page HTML
                var summary = await _openAiService.GenerateReportSummaryAsync(issues, htmlContent, url, cancellationToken);
                report.Summary = summary;
                _logger.LogInformation("Generated summary for report.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Summary generation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate summary.");
            }
        }

        private async Task MarkReportAsCompletedAsync(int reportId, CancellationToken cancellationToken)
        {
            try
            {
                // Retrieve a scoped instance of the DB context
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();

                // Find and update the report status to 'Completed'
                var report = await dbContext.Reports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
                if (report != null)
                {
                    report.Status = "Completed";
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Final save operation cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving final report status.");
            }
        }
    }
}