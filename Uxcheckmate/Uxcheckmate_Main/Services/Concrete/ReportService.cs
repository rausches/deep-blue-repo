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
        public ReportService(HttpClient httpClient, ILogger<ReportService> logger, UxCheckmateDbContext context, IOpenAiService openAiService, IBrokenLinksService brokenLinksService, IHeadingHierarchyService headingHierarchyService, IColorSchemeService colorSchemeService, IMobileResponsivenessService mobileResponsivenessService, IScreenshotService screenshotService, IPlaywrightScraperService playwrightScraperService, IPopUpsService popUpsService, IAnimationService animationService, IAudioService audioService, IScrollService scrollService, IFPatternService fPatternService, IZPatternService zPatternService, ISymmetryService symmetryService, IServiceScopeFactory scopeFactory)
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
            
            // Scrape site
            var fullScraped = await _playwrightScraperService.ScrapeEverythingAsync(url);
            var scrapedData = fullScraped.ToDictionary();

            // Get list of design categories
            var designCategories = await _dbContext.DesignCategories.ToListAsync();
            _logger.LogInformation("Found {Count} design categories.", designCategories.Count);

            // Use ConcurrentBag for thread-safe collection of results
            var scanResults = new ConcurrentBag<DesignIssue>();

            // Run analysis for each category in parallel
            await Parallel.ForEachAsync(
                designCategories,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 },
                async (category, cancellationToken) =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var scopedDbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();

                    _logger.LogInformation("Analyzing category: {CategoryName} using scan method: {ScanMethod}", category.Name, category.ScanMethod);

                    string message;
                    try
                    {
                        message = category.ScanMethod switch
                        {
                            "OpenAI" => await _openAiService.AnalyzeWithOpenAI(url, category.Name, category.Description, scrapedData),
                            "Custom" => await RunCustomAnalysisAsync(url, category.Name, category.Description, scrapedData, fullScraped),
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

                        // Save immediately using scoped context
                        scopedDbContext.DesignIssues.Add(designIssue);
                        await scopedDbContext.SaveChangesAsync(cancellationToken);

                        // Add to results collection
                        scanResults.Add(designIssue);
                        _logger.LogInformation("Design issue added for category {CategoryName} with severity {Severity}.", category.Name, designIssue.Severity);
                    }
                    else
                    {
                        _logger.LogInformation("No issues found for category: {CategoryName}", category.Name);
                    }
                });
                
                /* SAVE TOKENS COMMENT OUT OPEN AI  */

                // Call OpenAI to generate summary
                var summaryText = await _openAiService.GenerateReportSummaryAsync(scanResults.ToList(), fullScraped.HtmlContent, url);
                _logger.LogInformation("Generated summary: {Summary}", summaryText);
                report.Summary = summaryText;

                // Once ananlysis is complete update report status
                using (var finalScope = _scopeFactory.CreateScope())
                    {
                        var finalDbContext = finalScope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();
                        var finalReport = await finalDbContext.Reports.FirstOrDefaultAsync(r => r.Id == report.Id);
                        if (finalReport != null)
                        {
                            finalReport.Status = "Completed";
                            await finalDbContext.SaveChangesAsync();
                        }
                    }

            /* Commented out so that report instance will persist to view SEE QUEUE SERVICE FOR DELETION METHOD */

            /* if (report.Id > 0){
                // Add all design issues to the context
                _dbContext.DesignIssues.AddRange(scanResults);
                // Save to database
                await _dbContext.SaveChangesAsync(cancellationToken);
            }else{
                _logger.LogInformation("Skipping saving DesignIssues");    
            }*/


            // Return report
            return scanResults.ToList();
        }
        public async Task<string> RunCustomAnalysisAsync(string url, string categoryName, string categoryDescription, Dictionary<string, object> scrapedData, ScrapedContent fullScraped)
        {
            _logger.LogInformation("Running custom analysis for category: {CategoryName}", categoryName);
            Task<byte[]> screenshotTask = _screenshotService?.CaptureFullPageScreenshot(url) ?? Task.FromResult(new byte[0]);

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

            /* SAVE TOKENS COMMENT OUT OPEN AI */

            // Send to OpenAI to enhance message
         /*   if (!string.IsNullOrEmpty(message))
            {
                _logger.LogInformation("Improving message with OpenAI for category: {CategoryName}", categoryName);
                message = await _openAiService.ImproveMessageAsync(message, categoryName);
            }
            else
            {
                _logger.LogInformation("No message to improve for category: {CategoryName}", categoryName);
            }*/

            return message;
        }

     /*   private async Task<string> AnalyzeDynamicSizingAsync(string url, Dictionary<string, object> scrapedData)
        {
            bool hasDynamicSizing = _MobileResponsivenessIMobileResponsivenessService.HasDynamicSizing(scrapedData["htmlContent"].ToString());

            if (!hasDynamicSizing)
            {
                string prompt = $"The website at {url} is missing dynamic sizing elements (e.g., media queries, viewport meta tag, flexbox/grid layout). Please provide a recommendation on how to implement dynamic sizing.";
                return await _openAiService.AnalyzeWithOpenAI(url, "Dynamic Sizing", prompt, scrapedData);
            }

            _logger.LogDebug("Dynamic sizing elements are present. No recommendations needed.");
            return string.Empty;
        }*/
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
    }
}