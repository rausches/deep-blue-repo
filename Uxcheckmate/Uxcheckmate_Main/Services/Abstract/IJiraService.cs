using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IJiraService
    {
        Task<List<JiraProject>> GetProjectsAsync(string accessToken, string cloudId);
        Task ExportReportAsync(Report report, string accessToken, string cloudId, string projectKey);
    }
}