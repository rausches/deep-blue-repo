using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.EntityFrameworkCore;

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
        private readonly UxCheckmateDbContext _context;
        private readonly IOpenAiService _openAiService; 

        public JiraService(HttpClient httpClient, IOptions<JiraSettings> settings, UxCheckmateDbContext dbContext, 
        IOpenAiService openAiService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _context = dbContext;
            _openAiService = openAiService;
        }

        public async Task ExportReportAsync(Report report, string accessToken, string cloudId, string projectId)
        {
            // Loop through design issues and create Jira tasks
            foreach (var issue in report.DesignIssues)
            {
                if (string.IsNullOrEmpty(issue.Title))
                {
                    // Generate a title for the issue using OpenAI
                    issue.Title = await _openAiService.GenerateTitleAsync(issue.Message, issue.Category?.Name ?? "General");
                    await _context.SaveChangesAsync();
                }

                // Generate an improved description for the issue using OpenAI
                issue.Message = await _openAiService.GenerateImprovedDescriptionAsync(issue.Message, issue.Category?.Name ?? "General");

                await CreateJiraIssueAsync(accessToken, cloudId, projectId, issue.Title, issue.Message);
            }

            // Loop through accessibility issues and create Jira tasks
            foreach (var issue in report.AccessibilityIssues)
            {
                if (string.IsNullOrEmpty(issue.Title))
                {
                    // Generate a title for the issue using OpenAI
                    issue.Title = await _openAiService.GenerateAccTitleAsync(issue.Message, issue.Category?.Name ?? "General", issue.Selector);
                    await _context.SaveChangesAsync();
                }
                // Generate an improved description for the issue using OpenAI
                issue.Message = await _openAiService.GenerateImprovedDescriptionAsync(issue.Message, issue.Category?.Name ?? "General");

                await CreateJiraIssueAsync(accessToken, cloudId, projectId, issue.Title, issue.Message);
            }
        }

        public async Task<List<JiraProject>> GetProjectsAsync(string accessToken, string cloudId)
        {
            // Set the authorization header for the API call
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Build the Jira project search API endpoint
            var apiUrl = $"{_settings.ApiBaseUrl}ex/jira/{cloudId}/rest/api/3/project/search";

            // Call the Jira API to get projects
            var response = await _httpClient.GetAsync(apiUrl);

            // If the call fails, throw an exception with the error response
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Jira project fetch failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");

            // Read and deserialize the JSON response into a JiraProjectSearchResult object
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JiraProjectSearchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Return the list of projects, or an empty list if none are found
            return result?.Values ?? new List<JiraProject>();
        }

        private async Task CreateJiraIssueAsync(string accessToken, string cloudId, string projectId, string summary, string issueMessage)
        {
            var jiraIssue = new
            {
                fields = new
                {
                    project = new { id = projectId },   
                    summary = summary,
                    issuetype = new { name = "Task" },
                    description = new
                    {
                        type = "doc",
                        version = 1,
                        content = new object[]
                        {
                            new { type = "paragraph", content = new object[] { new { type = "text", text = $"Details: {issueMessage}" } } }
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
                throw new Exception($"Jira issue creation failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }

        private string CleanSummary(string input)
        {
            return string.IsNullOrEmpty(input) ? "" : input.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}
