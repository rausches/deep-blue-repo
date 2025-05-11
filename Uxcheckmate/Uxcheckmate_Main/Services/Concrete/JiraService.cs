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
            foreach (var issue in report.DesignIssues)
            {
                await CreateJiraIssueAsync(accessToken, cloudId, projectKey, $"[Design] {issue.Message}", issue.Severity, report.Url);
            }

            foreach (var issue in report.AccessibilityIssues)
            {
                await CreateJiraIssueAsync(accessToken, cloudId, projectKey, $"[Accessibility] {issue.Message}", issue.Severity, report.Url);
            }
        }

        private async Task CreateJiraIssueAsync(string accessToken, string cloudId, string projectKey, string summary, int severity, string reportUrl)
        {
            var jiraIssue = new
            {
                fields = new
                {
                    project = new { key = projectKey },
                    summary = summary,
                    description = $"Reported by UxCheckmate for URL: {reportUrl}\n\nSeverity: {severity}",
                    issuetype = new { name = "Task" }
                }
            };

            var json = JsonSerializer.Serialize(jiraIssue);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var apiUrl = $"{_settings.ApiBaseUrl}ex/jira/{cloudId}/rest/api/3/issue";
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Jira issue creation failed: {response.StatusCode} - {error}");
            }
        }
    }
}
