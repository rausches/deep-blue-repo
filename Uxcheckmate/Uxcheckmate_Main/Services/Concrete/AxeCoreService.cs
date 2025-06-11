using System.Text.Json;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.DTO;

namespace Uxcheckmate_Main.Services
{
    public class AxeCoreService : IAxeCoreService
    {
        private readonly ILogger<AxeCoreService> _logger;
        private readonly UxCheckmateDbContext _dbContext;
        private readonly IPlaywrightApiService _playwrightApiService;

        public AxeCoreService(
            ILogger<AxeCoreService> logger,
            UxCheckmateDbContext dbContext,
            IPlaywrightApiService playwrightApiService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _playwrightApiService = playwrightApiService;
        }

        public async Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report, CancellationToken cancellationToken = default)
        {
            var issues = new List<AccessibilityIssue>();

            try
            {
                _logger.LogInformation("Requesting accessibility analysis for URL: {Url}", report.Url);

                // Call external microservice
                var result = await _playwrightApiService.AnalyzeWebsiteAsync(report.Url);

                if (result?.AxeResults?.Violations == null || !result.AxeResults.Violations.Any())
                {
                    _logger.LogInformation("No accessibility violations found for {Url}.", report.Url);
                    return issues;
                }

                _logger.LogInformation("Found {Count} accessibility violations for {Url}.", result.AxeResults.Violations.Count, report.Url);

                // Get fallback category
                var defaultCategory = _dbContext.AccessibilityCategories.FirstOrDefault(c => c.Name == "Other");
                if (defaultCategory == null)
                {
                    _logger.LogError("Default accessibility category not found! Cannot categorize issues.");
                    return issues;
                }

                // Process violations into your DB model
                foreach (var violation in result.AxeResults.Violations)
                {
                    foreach (var node in violation.Nodes ?? new List<AxeNode>())
                    {
                        string categoryName = AccessibilityCategoryMapping.TryGetValue(violation.Id ?? "", out var mappedCategory)
                            ? mappedCategory
                            : "Other";

                        var category = _dbContext.AccessibilityCategories.FirstOrDefault(c => c.Name == categoryName)
                            ?? defaultCategory;

                        string details = node.FailureSummary ?? "No additional details available";

                        var issue = new AccessibilityIssue
                        {
                            ReportId = report.Id,
                            Message = violation.Description ?? violation.Id ?? "No description available",
                            Details = details,
                            Selector = node.Html ?? "No HTML available",
                            Severity = DetermineSeverity(violation.Impact),
                            WCAG = node.Target != null && node.Target.Any()
                                ? string.Join(", ", node.Target)
                                : "Unknown WCAG Rule",
                            CategoryId = category.Id
                        };

                        issues.Add(issue);
                    }
                }

                // Save to DB only if logged in
                if (!string.IsNullOrEmpty(report.UserID))
                {
                    _dbContext.AccessibilityIssues.AddRange(issues);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Saved {Count} accessibility issues to the database.", issues.Count);
                }
                else
                {
                    _logger.LogInformation("Anonymous scan. Issues generated but not saved to DB.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during external accessibility analysis.");
            }

            return issues;
        }

        protected int DetermineSeverity(string? impact) => impact switch
        {
            "critical" => 4,
            "serious" => 3,
            "moderate" => 2,
            _ => 1
        };

        // Reuse your existing mapping
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
    }
}
