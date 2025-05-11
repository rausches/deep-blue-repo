using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uxcheckmate_Main.Services; 
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JiraAPIController : ControllerBase
    {
        private readonly UxCheckmateDbContext _dbContext;
        private readonly IJiraService _jiraService;

        public JiraController(UxCheckmateDbContext dbContext, IJiraService jiraService)
        {
            _dbContext = dbContext;
            _jiraService = jiraService;
        }

        [HttpPost("ExportReportToJira")]
        public async Task<IActionResult> ExportReportToJira(int reportId)
        {
            // Query the database for the report by ID
            // Also eagerly load the related DesignIssues and AccessibilityIssues
            var report = await _dbContext.Reports
                .Include(r => r.DesignIssues)
                .Include(r => r.AccessibilityIssues)
                .FirstOrDefaultAsync(r => r.Id == reportId);

            // If report does not exist, return 404 Not Found
            if (report == null)
                return NotFound("Report not found.");

            // Pass the report to the Jira service to handle the export
            await _jiraService.ExportReportAsync(report);

            // Return a 200 OK response with a confirmation message
            return Ok(new { message = "Report exported to Jira." });
        }
    }
}
