using System.Text.Json;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace Uxcheckmate_Main.Services
{
    public class AxeCoreService : IAxeCoreService
    {
        private readonly ILogger<AxeCoreService> _logger;
        protected readonly UxCheckmateDbContext _dbContext;
        private readonly IPlaywrightService _playwrightService;

        public AxeCoreService(ILogger<AxeCoreService> logger, UxCheckmateDbContext dbContext, IPlaywrightService playwrightService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _playwrightService = playwrightService;
        }

        public virtual async Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report, CancellationToken cancellationToken = default)
        {
            var issues = new List<AccessibilityIssue>();
            IBrowserContext context = null;

            try
            {
                _logger.LogInformation("Starting accessibility analysis for URL: {Url}", report.Url);

                // Request a new browser context from the PlaywrightService.
                context = await _playwrightService.GetBrowserContextAsync();
                var page = await context.NewPageAsync();

                // Navigate to the target URL and wait until the page is fully loaded
                await LoadTargetPageAsync(page, report.Url);
                
                // Construct the file path for the axe-core script
                string axeScript = await LoadAxeScriptAsync();
                await InjectAxeScriptAsync(page, axeScript);
                ValidateAxeInjection(page);

                // Execute axe.run() to analyze the page
                var axeResults = await RunAxeAnalysisAsync(page);
                if (axeResults?.Violations == null || !axeResults.Violations.Any())
                {
                    _logger.LogInformation("No accessibility violations found for {Url}.", report.Url);
                    return issues;
                }

                // Process each accessibility violation and create issue records
                issues = await ProcessViolationsAsync(axeResults, report);

                await SaveIssuesIfAuthenticatedAsync(issues, report);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during accessibility analysis: {Message}", ex.Message);
            }
            finally
            {
                // Close the browser context for this run so sessions don't overlap.
                if (context != null)
                    await context.CloseAsync();
            }

            return issues;
        }

        private async Task LoadTargetPageAsync(IPage page, string url)
        {
            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });
            _logger.LogInformation("Successfully loaded page: {Url}", url);
        }

        private async Task<string> LoadAxeScriptAsync()
        {
            // Construct the file path for the axe-core script
            string path = Path.Combine(Directory.GetCurrentDirectory(), "axe-core", "axe.min.js");

            // Verify that the axe-core script exists before injecting it
            if (!File.Exists(path))
                throw new FileNotFoundException("Axe-core script not found", path);

            return await File.ReadAllTextAsync(path);
        }

        private async Task InjectAxeScriptAsync(IPage page, string axeScript)
        {
            await page.EvaluateAsync(axeScript);
            _logger.LogInformation("Injected axe-core script successfully.");
        }

        private async void ValidateAxeInjection(IPage page)
        {
            var isAxeDefined = await page.EvaluateAsync<bool>("() => typeof axe !== 'undefined'");
            if (!isAxeDefined)
                throw new Exception("Axe-core script injection failed");
            _logger.LogInformation("Axe-core script is loaded. Running accessibility analysis...");
        }

        private async Task<AxeCoreResults?> RunAxeAnalysisAsync(IPage page)
        {
            var json = await page.EvaluateAsync<JsonElement>(@"
                async () => {
                    let results = await axe.run();
                    results.violations.forEach(violation => {
                        violation.nodes.forEach(node => {
                            if (node.target.length > 0) {
                                let element = document.querySelector(node.target[0]);
                                node.html = element ? element.outerHTML : 'Element not found';
                            } else {
                                node.html = 'No target selector available';
                            }
                        });
                    });
                    return results;
                }");

            // Deserialize the JSON result into an AxeCoreResults object
            return JsonSerializer.Deserialize<AxeCoreResults>(json.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<List<AccessibilityIssue>> ProcessViolationsAsync(AxeCoreResults axeResults, Report report)
        {
            var issues = new List<AccessibilityIssue>();

            // Ensure that the default "Other" category exists
            var defaultCategory = _dbContext.AccessibilityCategories.FirstOrDefault(c => c.Name == "Other");
            if (defaultCategory == null)
                throw new InvalidOperationException("Default accessibility category not found!");

            // Process each accessibility violation and create issue records
            foreach (var violation in axeResults.Violations)
            {
                foreach (var node in violation.Nodes)
                {
                    string categoryName = AccessibilityCategoryMapping.TryGetValue(violation.Id, out var mappedCategory) ? mappedCategory : "Other";

                    var category = _dbContext.AccessibilityCategories.FirstOrDefault(c => c.Name == categoryName)
                                ?? defaultCategory;

                    issues.Add(new AccessibilityIssue
                    {
                        ReportId = report.Id,
                        Message = violation.Help ?? violation.Description ?? "No description available",
                        Details = node.FailureSummary ?? "No additional details available",
                        Selector = node.Html ?? "No HTML available",
                        Severity = DetermineSeverity(violation.Impact),
                        WCAG = violation.WcagTags?.Any() == true ? string.Join(", ", violation.WcagTags) : "Unknown WCAG Rule",
                        CategoryId = category?.Id ?? 0
                    });
                }
            }

            _logger.LogInformation("Found {Count} accessibility violations for {Url}.", issues.Count, report.Url);
            return issues;
        }

        private async Task SaveIssuesIfAuthenticatedAsync(List<AccessibilityIssue> issues, Report report)
        {
            if (!string.IsNullOrEmpty(report.UserID))
            {
                _logger.LogInformation("Saving {Count} accessibility issues to the database.", issues.Count);
                _dbContext.AccessibilityIssues.AddRange(issues);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("All issues saved successfully!");
            }
            else
            {
                _logger.LogInformation("User not logged in, issues generated but not saved to DB.");
            }
        }

        protected static readonly Dictionary<string, string> AccessibilityCategoryMapping = new()
        {
            { "color-contrast", "Color & Contrast" },
            { "focus-order", "Keyboard & Focus" },
            { "bypass", "Page Structure & Landmarks" },
            { "landmark-one-main", "Page Structure & Landmarks" },
            { "label", "Forms & Inputs" },
            { "button-name", "Link & Buttons" },
            { "link-name", "Link & Buttons" },
            { "audio-caption", "Multimedia & Animations" },
            { "video-caption", "Multimedia & Animations" },
            { "image-alt", "Multimedia & Animations" },
            { "input-image-alt", "Multimedia & Animations" },
            { "image-redundant-alt", "Multimedia & Animations" },
            { "role-img-alt", "Multimedia & Animations"},
            { "meta-refresh", "Timeouts & Auto-Refresh" },
            { "scrollable-region-focusable", "Motion & Interaction" },
            { "aria-allowed-attr", "ARIA & Semantic HTML" },
            { "aria-hidden-focus", "ARIA & Semantic HTML" }
        };

        protected int DetermineSeverity(string impact)
        {
            return impact switch
            {
                "critical" => 4,
                "serious" => 3,
                "moderate" => 2,
                _ => 1

            };
        }
    }
}