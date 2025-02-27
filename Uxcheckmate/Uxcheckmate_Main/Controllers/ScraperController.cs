/*using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Controllers
{
    [ApiController]
    [Route("scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly WebScraperService _scraperService;
        private readonly PdfService _pdfService;

        public ScraperController(WebScraperService scraperService, PdfService pdfService)
        {
            _scraperService = scraperService;
            _pdfService = pdfService;
        }

        [HttpGet("extract")]
        public async Task<IActionResult> Extract([FromQuery] string url, [FromQuery] bool exportPdf = false)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { error = "URL parameter is required." });
            }

            try
            {
                var extractedData = await _scraperService.ScrapeAsync(url); // ✅ Ensure this is a Dictionary<string, object>

                if (exportPdf)
                {
                    var report = new Report
                    {
                        Url = url,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow),
                        AccessibilityIssues = new List<AccessibilityIssue>(), // Populate with real data
                        DesignIssues = new List<DesignIssue>() // Populate with real data
                    };

                    var pdfBytes = _pdfService.GenerateReportPdf(report);
                    return File(pdfBytes, "application/pdf", "UX_Report.pdf");
                }

                return Ok(new
                {
                    Url = url,
                    Headings = extractedData.TryGetValue("headings", out var headings) ? headings : 0,
                    Images = extractedData.TryGetValue("images", out var images) ? images : 0,
                    Links = extractedData.TryGetValue("links", out var links) ? links : 0
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
*/