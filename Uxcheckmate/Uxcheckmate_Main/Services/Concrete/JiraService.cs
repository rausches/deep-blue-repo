using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{

    public class JiraSettings
    {
        public string ApiBaseUrl { get; set; } = "https://api.atlassian.com/";
    }

    public class JiraService : IJiraService
    {
        private readonly HttpClient _httpClient;
        private readonly JiraSettings _settings;

        public JiraService(HttpClient httpClient, IOptions<JiraSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task ExportReportAsync(Report report, string accessToken, string cloudId, string projectKey)
        {
            // Loop through design issues and create Jira tasks
            foreach (var issue in report.DesignIssues)
            {
                await CreateJiraIssueAsync(
                    accessToken,
                    cloudId,
                    projectKey,
                     // Add prefix to clarify issue type
                    $"[Design] {CleanSummary(issue.Message)}",
                    issue.Severity,
                    report.Url
                );
            }

            // Loop through accessibility issues and create Jira tasks
            foreach (var issue in report.AccessibilityIssues)
            {
                await CreateJiraIssueAsync(
                    accessToken,
                    cloudId,
                    projectKey,
                    $"[Accessibility] {CleanSummary(issue.Message)}",
                    issue.Severity,
                    report.Url
                );
            }
        }

        private async Task CreateJiraIssueAsync(string accessToken, string cloudId, string projectKey, string summary, int severity, string reportUrl)
        {
            // Create the Jira issue payload using Atlassian Document Format (ADF)
            var jiraIssue = new
            {
                fields = new
                {
                    project = new { key = projectKey },
                    summary = summary,
                    issuetype = new { name = "Task" },
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new object[]
                        {
                            new
                            {
                                type = "heading",
                                attrs = new { level = 3 },
                                content = new object[]
                                {
                                    new { type = "text", text = "Issue Details" }
                                }
                            },
                            new
                            {
                                type = "paragraph",
                                content = new object[]
                                {
                                    new { type = "text", text = "This issue was detected by UxCheckmate." }
                                }
                            },
                            new
                            {
                                type = "bulletList",
                                content = new object[]
                                {
                                    CreateListItem("Report URL", reportUrl),
                                    CreateListItem("Severity", severity.ToString()),
                                    CreateListItem("Tool", "UxCheckmate"),
                                }
                            }
                        }
                    }
                }
            };

            // Serialize the payload to JSON
            var json = JsonSerializer.Serialize(jiraIssue);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Set the authorization header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Build Jira API URL and post the issue
            var apiUrl = $"{_settings.ApiBaseUrl}ex/jira/{cloudId}/rest/api/3/issue";
            var response = await _httpClient.PostAsync(apiUrl, content);

            // Throw an exception if the request fails
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Jira issue creation failed: {response.StatusCode} - {error}");
            }
        }

        private object CreateListItem(string label, string value)
        {
            return new
            {
                type = "listItem",
                content = new object[]
                {
                    new
                    {
                        type = "paragraph",
                        content = new object[]
                        {
                            new { type = "text", text = $"{label}: {value}" }
                        }
                    }
                }
            };
        }

        private string CleanSummary(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : input.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
