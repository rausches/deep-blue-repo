using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Uxcheckmate_Main.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient; 
    private readonly UxCheckmateDbContext _context;
    private readonly IOpenAiService _openAiService; 
    private readonly IReportService _reportService;
    private readonly IAxeCoreService _axeCoreService;
    private readonly PdfExportService _pdfExportService;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, UxCheckmateDbContext dbContext, 
        IOpenAiService openAiService, IAxeCoreService axeCoreService, IReportService reportService, PdfExportService pdfExportService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = dbContext;
        _axeCoreService = axeCoreService;
        _reportService = reportService;
        _pdfExportService = pdfExportService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

[HttpPost]
    public async Task<IActionResult> Report(string url, string sortOrder = "category", bool isAjax = false)
    {
        if (string.IsNullOrEmpty(url))
        {
            ModelState.AddModelError("url", "URL cannot be empty.");
            return View("Index");
        }

       try
        {
            // Check if the URL is reachable
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Sending a HEAD request to the URL to check if it is reachable
                HttpResponseMessage response;
                try{
                    response = await httpClient.SendAsync(request);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "The URL is unreachable: {Url}", url);
                    TempData["UrlUnreachable"] = "The URL you entered seems incorrect or no longer exists. Please try again.";
                    return RedirectToAction("Index");
                }
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("The URL is unreachable: {Url}", url);
                    TempData["UrlUnreachable"] = "The URL you entered seems incorrect or no longer exists. Please try again.";
                    return RedirectToAction("Index");
                }
            }
            string? userId = null;
            if (User.Identity.IsAuthenticated){
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            // Create and save the report record.
            var report = new Report
            {
                Url = url,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                UserID = userId,
                AccessibilityIssues = new List<AccessibilityIssue>(),
                DesignIssues = new List<DesignIssue>()
            };
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Report record created with ID: {ReportId}", report.Id);
            
            // Run analysis
            await _axeCoreService.AnalyzeAndSaveAccessibilityReport(report);
            await _reportService.GenerateReportAsync(report);

            // Fetch the full report 
            var fullReport = await _context.Reports
            .Include(r => r.AccessibilityIssues)
                .ThenInclude(a => a.Category)
            .Include(r => r.DesignIssues)
                .ThenInclude(d => d.Category)
            .FirstOrDefaultAsync(r => r.Id == report.Id);

            if (fullReport == null)
            {
                _logger.LogError("Failed to fetch report with ID: {ReportId}", report.Id);
                ModelState.AddModelError("", "An error occurred while fetching the report.");
                return View("Index");
            }

        // Apply sorting
        ViewBag.CurrentSort = sortOrder;
        
        fullReport.DesignIssues = sortOrder switch
        {
            "severity-high-low" => fullReport.DesignIssues
                .OrderByDescending(i => i.Severity)
                .ThenBy(i => i.Category.Name)
                .ToList(),
            "severity-low-high" => fullReport.DesignIssues
                .OrderBy(i => i.Severity)
                .ThenBy(i => i.Category.Name)
                .ToList(),
            _ => fullReport.DesignIssues
                .OrderBy(i => i.Category.Name)
                .ThenByDescending(i => i.Severity)
                .ToList()
        };

        fullReport.AccessibilityIssues = sortOrder switch
        {
            "severity-high-low" => fullReport.AccessibilityIssues
                .OrderByDescending(i => i.Severity)
                .ThenBy(i => i.Category.Name)
                .ToList(),
            "severity-low-high" => fullReport.AccessibilityIssues
                .OrderBy(i => i.Severity)
                .ThenBy(i => i.Category.Name)
                .ToList(),
            _ => fullReport.AccessibilityIssues
                .OrderBy(i => i.Category.Name)
                .ThenByDescending(i => i.Severity)
                .ToList()
        };

        if (isAjax)
        {
            return PartialView("_ReportSections", fullReport);
        }

        return View("Results", fullReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for URL: {Url}", url);
            ModelState.AddModelError("", $"An error occurred: {ex.Message}");
            return View("Index");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Feedback()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ErrorPage()
    {
        return View("ErrorPage");
    }
    [Authorize] // Have to be logged in/Authorized to access
    public IActionResult UserDash()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public async Task<IActionResult> DownloadReport(int id)
    {
        var report = await _context.Reports
            .Include(r => r.AccessibilityIssues)
            .Include(r => r.DesignIssues)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null)
        {
            return NotFound("Report not found.");
        }

        var pdfBytes = _pdfExportService.GenerateReportPdf(report);
        return File(pdfBytes, "application/pdf", $"UXCheckmate_Report_{report.Id}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> GetSortedIssues(int id, string sortOrder)
    {
        var report = await _context.Reports
            .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
            .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (report == null) return NotFound();

        // Add ViewBag value for partials
        ViewBag.CurrentSort = sortOrder;

        // Sort design issues
        report.DesignIssues = sortOrder switch
        {
            "severity-high-low" => report.DesignIssues.OrderByDescending(i => i.Severity).ToList(),
            "severity-low-high" => report.DesignIssues.OrderBy(i => i.Severity).ToList(),
            _ => report.DesignIssues.OrderBy(i => i.Category.Name).ToList()
        };

        // Sort accessibility issues
        report.AccessibilityIssues = sortOrder switch
        {
            "severity-high-low" => report.AccessibilityIssues.OrderByDescending(i => i.Severity).ToList(),
            "severity-low-high" => report.AccessibilityIssues.OrderBy(i => i.Severity).ToList(),
            _ => report.AccessibilityIssues.OrderBy(i => i.Category.Name).ToList()
        };

        var designHtml = await this.RenderViewAsync("_DesignIssuesPartial", report.DesignIssues);
        var accessibilityHtml = await this.RenderViewAsync("_AccessibilityIssuesPartial", report.AccessibilityIssues);

        return Json(new { designHtml, accessibilityHtml });
    }

}

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model)
    {
        controller.ViewData.Model = model;
        using var writer = new StringWriter();
        
        var viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>();
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

        if (!viewResult.Success)
            throw new Exception($"View '{viewName}' not found");

        var viewContext = new ViewContext(
            controller.ControllerContext,
            viewResult.View,
            controller.ViewData,
            controller.TempData,
            writer,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return writer.ToString();
    }
}