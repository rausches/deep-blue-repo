using System.Text.Json;
using System.Collections.Generic;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;
using Microsoft.AspNetCore.Hosting;

/*namespace Uxcheckmate_Main.Services
{
   public class Pa11yService : IPa11yService
    {
        private readonly Pa11yUrlBasedService _pa11yUrlBasedService;
        private readonly ILogger<Pa11yService> _logger;
        private readonly UxCheckmateDbContext _dbContext;

        public Pa11yService(Pa11yUrlBasedService pa11yUrlBasedService, ILogger<Pa11yService> logger, UxCheckmateDbContext dbContext)
        {
            _pa11yUrlBasedService = pa11yUrlBasedService;
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report)
        {
            var url = report.Url;
                        
            // Ensure the AccessibilityIssues collection is not null.
            var scanResults = report.AccessibilityIssues ?? new List<AccessibilityIssue>();
            _logger.LogInformation("Starting report generation for URL: {Url}", url);

            // Run Pa11y using the existing service and get the result as a JSON string
            var pa11yJsonResult = await Task.Run(() => _pa11yUrlBasedService.RunPa11y(url));

            // Log the JSON result
            _logger.LogError("Pa11y JSON Result: {Pa11yJsonResult}", pa11yJsonResult);
            

            // Deserialize the JSON result into a collection of dictionaries
            var pa11yResultsCollection = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(pa11yJsonResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Dictionary<string, object>>();

            // Fetch all predefined categories from the database
            var categories = await _dbContext.AccessibilityCategories.ToListAsync();

            foreach (var issue in pa11yResultsCollection)
            {
                // Extract fields dynamically with null checks
                string message = issue.ContainsKey("message") && issue["message"] != null
                    ? issue["message"].ToString()
                    : "Unknown issue";
                string selector = issue.ContainsKey("selector") && issue["selector"] != null
                    ? issue["selector"].ToString()
                    : "Unknown element";
                string wcagLabel = issue.ContainsKey("code") && issue["code"] != null
                    ? issue["code"].ToString()
                    : "Unknown WCAG Rule";
                string type = issue.ContainsKey("type") && issue["type"] != null
                    ? issue["type"].ToString()
                    : "notice"; // Default to notice

                // Call ExtractWCAGLabel() to clean message and extract WCAG (if possible)
                var (cleanedMessage, extractedWCAG) = ExtractWCAGLabel(message);
                if (extractedWCAG != "Unknown WCAG Rule" || !string.IsNullOrWhiteSpace(wcagLabel))
                {
                    wcagLabel = extractedWCAG != "Unknown WCAG Rule" ? extractedWCAG : wcagLabel;
                }

                // Assign severity based on Pa11y’s type
                int severityLevel = type switch
                {
                    "error" => 3,
                    "warning" => 2,
                    _ => 1 // "notice" = low severity
                };

                // Categorize the issue dynamically
                string catName = CategorizePa11yIssue(cleanedMessage, categories);
                var category = categories.FirstOrDefault(c => c.Name == catName)
                            ?? categories.FirstOrDefault(c => c.Name == "Other");


                // Create a new AccessibilityIssue
                var accessibilityIssue = new AccessibilityIssue
                {
                    CategoryId = category.Id,
                    ReportId = report.Id,
                    Message = cleanedMessage,
                    WCAG = wcagLabel,
                    Selector = selector,
                    Severity = severityLevel,
                    Category = category
                };

                scanResults.Add(accessibilityIssue);
            }


            // Save issues to the database
            _dbContext.AccessibilityIssues.AddRange(scanResults);
            await _dbContext.SaveChangesAsync();


            return scanResults;
        }

        private string CategorizePa11yIssue(string message, List<AccessibilityCategory> categories)
        {
            var categoryMappings = new Dictionary<string, string>
            {
                { "contrast", "Color & Contrast" },
                { "color", "Color & Contrast" },
                { "keyboard", "Keyboard & Focus" },
                { "focus", "Keyboard & Focus" },
                { "tab", "Keyboard & Focus" },
                { "input", "Forms & Inputs" },
                { "form", "Forms & Inputs" },
                { "label", "Forms & Inputs" },
                { "button", "Link & Buttons" },
                { "link", "Link & Buttons" },
                { "alt text", "Multimedia & Animations" },
                { "caption", "Multimedia & Animations" },
                { "autoplay", "Multimedia & Animations" },
                { "heading", "Page Structure & Landmarks" },
                { "landmark", "Page Structure & Landmarks" },
                { "title", "Page Structure & Landmarks" },
                { "timeout", "Timeouts & Auto-Refresh" },
                { "auto-refresh", "Timeouts & Auto-Refresh" },
                { "motion", "Motion & Interaction" },
                { "scroll", "Motion & Interaction" },
                { "aria", "ARIA & Semantic HTML" }
            };


            // Match the message against category mappings
            foreach (var mapping in categoryMappings)
            {
                if (message.Contains(mapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return mapping.Value;
                }
            }


            return "Other"; // Default category if no match
        }
        private static (string cleanedMessage, string wcagLabel) ExtractWCAGLabel(string message)
        {
            // Match WCAG label inside parentheses
            var match = Regex.Match(message, @"\((WCAG [^\)]+)\)");


            if (match.Success)
            {
                string wcagLabel = match.Groups[1].Value; // Extract WCAG label
                string cleanedMessage = Regex.Replace(message, @"\s*\(WCAG.*?\)", "", RegexOptions.IgnoreCase).Trim(); // Remove WCAG reference
                return (cleanedMessage, wcagLabel);
            }


            return (message, "Unknown WCAG Rule"); // If no match, assign "Unknown"
        }

        private List<Pa11yIssue> DeserializePa11yResult(string pa11yJsonResult)
        {
            try
            {
                // Deserialize the JSON result into a List of Pa11yIssue
                var issues = JsonSerializer.Deserialize<List<Pa11yIssue>>(pa11yJsonResult);

                return issues;
            }
            catch (JsonException ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error deserializing Pa11y JSON result");

                // Handle any JSON deserialization errors
                return new List<Pa11yIssue>();
            }
        }
    }
}*/


namespace Uxcheckmate_Main.Services
{
    public class AxeCoreService : IAxeCoreService
    {
        private readonly ILogger<AxeCoreService> _logger;
        private readonly UxCheckmateDbContext _dbContext;

        public AxeCoreService(ILogger<AxeCoreService> logger, UxCheckmateDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report)
        {
            var issues = new List<AccessibilityIssue>();

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();

            try
            {
                _logger.LogInformation("Navigating to {Url}", report.Url);
                await page.GotoAsync(report.Url, new PageGotoOptions { WaitUntil = WaitUntilState.Load });

                // Inject axe-core script (downloaded locally)
                // Get the absolute path to axe.min.js
                string axeScriptPath = Path.Combine(Directory.GetCurrentDirectory(), "axe-core", "axe.min.js");
                _logger.LogInformation("Attempting to load axe-core script from: {Path}", axeScriptPath);

                // Check if the file exists
                if (!File.Exists(axeScriptPath))
                {
                    _logger.LogError("axe-core script not found at path: {Path}", axeScriptPath);
                    throw new FileNotFoundException("axe-core script not found", axeScriptPath);
                }

                string axeScript = await File.ReadAllTextAsync(axeScriptPath);
                await page.EvaluateAsync(axeScript);
                _logger.LogInformation("Successfully injected axe-core script.");

                // Run axe-core test and check for errors
                var axeCheck = await page.EvaluateAsync<bool>("() => typeof axe !== 'undefined'");
                if (!axeCheck)
                {
                    _logger.LogError("Axe-core script injection failed. Axe is undefined.");
                    throw new Exception("Axe-core script injection failed");
                }

                _logger.LogInformation("Axe-core script is successfully injected. Running analysis...");

                await page.EvaluateAsync(axeScript);

                // Run accessibility scan
                try
                {
                    var axeResultsJson = await page.EvaluateAsync<JsonElement>("() => axe.run()");
                    string jsonString = axeResultsJson.ToString();

                    _logger.LogInformation("Raw axe-core JSON output: {JsonString}", jsonString);
                    
                    var axeResults = JsonSerializer.Deserialize<AxeCoreResults>(jsonString, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (axeResults?.Violations == null || !axeResults.Violations.Any())
                        {
                            _logger.LogError("Axe-core returned no violations after deserialization.");
                        }
                    if (axeResults == null || axeResults.Violations == null)
                    {
                        _logger.LogError("Axe-core returned no results or deserialization failed.");
                    }

                    if (axeResults?.Violations != null)
                    {
                        foreach (var violation in axeResults.Violations)
                        {
                            var issue = new AccessibilityIssue
                            {
                                ReportId = report.Id,
                                Message = violation.Description,
                                Selector = string.Join(", ", violation.Nodes.Select(n => n.Target)),
                                Severity = DetermineSeverity(violation.Impact),
                                WCAG = string.Join(", ", violation.WcagTags)
                            };

                            issues.Add(issue);
                        }

                        // Save issues to database (Just like Pa11yService does)
                        _dbContext.AccessibilityIssues.AddRange(issues);
                        await _dbContext.SaveChangesAsync();
                        _logger.LogInformation("Saved {Count} accessibility issues to the database.", issues.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error executing axe.run(): {Message}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error running Playwright accessibility test: {Message}", ex.Message);
            }

            return issues; // Return accessibility issues just like Pa11yService
        }

        private int DetermineSeverity(string impact)
        {
            return impact switch
            {
                "critical" => 3,
                "serious" => 2,
                _ => 1
            };
        }
        private class AxeCoreResults
{
    public List<AxeViolation> Violations { get; set; }
}

private class AxeViolation
{
    public string Id { get; set; }  // Matches "id" field in JSON
    public string Impact { get; set; } // Matches "impact" field in JSON
    public string Description { get; set; } // Matches "description" field
    public string Help { get; set; } // Matches "help" field
    public List<string> WcagTags { get; set; }
    public List<AxeNode> Nodes { get; set; } // Matches "nodes" field
}

private class AxeNode
{
    public string Html { get; set; }  // Matches "html" field
    public List<string> Target { get; set; }  // Matches "target" field (CSS selector)
    public List<AxeCheck> Any { get; set; }  // Matches "any" field
    public List<AxeCheck> All { get; set; }  // Matches "all" field
    public List<AxeCheck> None { get; set; }  // Matches "none" field
}

private class AxeCheck
{
    public string Id { get; set; }  // Matches "id" inside "any", "all", "none"
    public string Message { get; set; }  // Matches "message"
}

    }
}


