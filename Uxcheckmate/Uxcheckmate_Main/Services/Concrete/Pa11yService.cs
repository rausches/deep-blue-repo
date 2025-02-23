using System.Text.Json;
using System.Collections.Generic;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace Uxcheckmate_Main.Services
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
            _logger.LogInformation("Pa11y JSON Result: {Pa11yJsonResult}", pa11yJsonResult);

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
}
