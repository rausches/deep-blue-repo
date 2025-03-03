using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
/*
[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) // ✅ Use the interface
    {
        _reportService = reportService;
    }

  /*  [HttpGet("download")]
    public async Task<IActionResult> DownloadReport([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest("URL is required.");
        }

        var report = new Report { Url = url }; // Create a report object
        var pdfBytes = await _reportService.GenerateReportPdfAsync(report);

        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            return NotFound("Failed to generate PDF.");
        }

        return File(pdfBytes, "application/pdf", "UX_Report.pdf");
    }*/
}
*/