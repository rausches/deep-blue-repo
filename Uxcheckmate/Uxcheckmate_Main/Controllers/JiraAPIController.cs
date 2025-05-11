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

        public JiraAPIController(UxCheckmateDbContext dbContext, IJiraService jiraService)
        {
            _dbContext = dbContext;
            _jiraService = jiraService;
        }

        [HttpGet("IsConnected")]
        public IActionResult IsConnected()
        {
            var accessToken = HttpContext.Session.GetString("JiraAccessToken");
            var cloudId = HttpContext.Session.GetString("JiraCloudId");

            bool connected = !string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(cloudId);
            return Ok(new { connected });
        }

        [HttpPost("ExportReportToJira")]
        public async Task<IActionResult> ExportReportToJira(int reportId, string projectKey)
        {
            var accessToken = HttpContext.Session.GetString("JiraAccessToken");
            var cloudId = HttpContext.Session.GetString("JiraCloudId");

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(cloudId))
                return BadRequest("You must connect Jira first.");

            if (string.IsNullOrEmpty(projectKey))
                return BadRequest("Project key is required.");

            var report = await _dbContext.Reports
                .Include(r => r.DesignIssues)
                .Include(r => r.AccessibilityIssues)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            if (report == null)
                return NotFound("Report not found.");

            await _jiraService.ExportReportAsync(report, accessToken, cloudId, projectKey);

            return Ok(new { message = "Report exported to Jira." });
        }

    }
}
