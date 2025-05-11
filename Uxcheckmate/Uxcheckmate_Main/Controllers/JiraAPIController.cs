using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Main.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("JiraAPI")]
    public class JiraAPIController : ControllerBase
    {
        private readonly UxCheckmateDbContext _dbContext;
        private readonly IJiraService _jiraService;

        // Constructor with DI for DbContext and Jira service
        public JiraAPIController(UxCheckmateDbContext dbContext, IJiraService jiraService)
        {
            _dbContext = dbContext;
            _jiraService = jiraService;
        }

        [HttpGet("IsConnected")]
        public IActionResult IsConnected()
        {
            // Retrieve Jira connection info from session
            var accessToken = HttpContext.Session.GetString("JiraAccessToken");
            var cloudId = HttpContext.Session.GetString("JiraCloudId");

            // User is considered connected if both values are present
            bool connected = !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(cloudId);
            return Ok(new { connected });
        }

        [HttpPost("ExportReportToJira")]
        public async Task<IActionResult> ExportReportToJira(int reportId, string projectKey)
        {
            // Retrieve Jira connection info from session
            var accessToken = HttpContext.Session.GetString("JiraAccessToken");
            var cloudId = HttpContext.Session.GetString("JiraCloudId");

            // Validate connection
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(cloudId))
                return BadRequest("You must connect Jira first.");

            // Validate input
            if (string.IsNullOrEmpty(projectKey))
                return BadRequest("Project key is required.");

            // Find the report by ID including its related issues
            var report = await _dbContext.Reports
                .Include(r => r.DesignIssues)
                .Include(r => r.AccessibilityIssues)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            // Return 404 if no matching report found
            if (report == null)
                return NotFound("Report not found.");

            // Call Jira service to create Jira issues for each problem in the report
            await _jiraService.ExportReportAsync(report, accessToken, cloudId, projectKey);

            // Respond with success message
            return Ok(new { message = "Report exported to Jira." });
        }
    }
}
