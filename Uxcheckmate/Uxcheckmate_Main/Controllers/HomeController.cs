using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Playwright;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.IO;

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
    private readonly IScreenshotService _screenshotService;
    private readonly IViewRenderService _viewRenderService;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, UxCheckmateDbContext dbContext, 
        IOpenAiService openAiService, IAxeCoreService axeCoreService, IReportService reportService, 
        PdfExportService pdfExportService, IScreenshotService screenshotService, IViewRenderService viewRenderService)
        
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = dbContext;
        _axeCoreService = axeCoreService;
        _reportService = reportService;
        _pdfExportService = pdfExportService;
        _screenshotService = screenshotService;
        _viewRenderService = viewRenderService;
    }
    public IActionResult Index()
    {

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Report(string url, string sortOrder = "category", bool isAjax = false)
    {
        if (!ValidateUrl(url))
        {
            return View("Index");
        }

        if (!await IsUrlReachable(url))
        {
            TempData["UrlUnreachable"] = "The URL you entered seems incorrect or no longer exists. Please try again.";
            return RedirectToAction("Index");
        }

        var websiteScreenshot = await CaptureScreenshot(url);

        if (string.IsNullOrEmpty(websiteScreenshot))
        {
            ModelState.AddModelError("", "An error occurred while capturing the screenshot.");
            return View("Index");
        }
        TempData["WebsiteScreenshot"] = websiteScreenshot;


        // Create and save the report record.
        var report = await CreateAndSaveReport(url);

        // Run analysis and generate the report
        await RunAnalysis(report);

        // Fetch the full report including related issues and categories
        var fullReport = await FetchFullReport(report.Id);
        if (fullReport == null)
        {
            ModelState.AddModelError("", "An error occurred while fetching the report.");
            return View("Index");
        }
        // Sort the report issues
        SortReportIssues(fullReport, sortOrder);

        ViewBag.CurrentSort = sortOrder;

        return View("Results", fullReport);
    }

    private bool ValidateUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            ModelState.AddModelError("url", "URL cannot be empty.");
            return false;
        }
        return true;
    }

    private async Task<bool> IsUrlReachable(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            _logger.LogInformation("Request Headers: {Headers}", request.Headers);

            var response = await httpClient.SendAsync(request);
            _logger.LogInformation("Response Headers: {Headers}", response.Headers);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "The URL is unreachable: {Url}", url);
            return false;
        }
    }

    private async Task<string?> CaptureScreenshot(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            _logger.LogError("URL is empty or null.");
            return null;
        }

        try
        {
            var screenshotOptions = new PageScreenshotOptions { FullPage = true };
            var screenshot = await _screenshotService.CaptureScreenshot(screenshotOptions, url);

            if (string.IsNullOrEmpty(screenshot))
            {
                _logger.LogError("Failed to capture screenshot for URL: {Url}", url);
                return null;
            }

            return screenshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while capturing the screenshot for URL: {Url}", url);
            return null;
        }
    }
    private async Task<Report> CreateAndSaveReport(string url)
    {
        string? userId = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

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

        return report;
    }

    private async Task RunAnalysis(Report report)
    {
        await _axeCoreService.AnalyzeAndSaveAccessibilityReport(report);
        await _reportService.GenerateReportAsync(report);
    }

    private async Task<Report?> FetchFullReport(int reportId)
    {
        return await _context.Reports
            .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
            .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
            .FirstOrDefaultAsync(r => r.Id == reportId);
    }

    private void SortReportIssues(Report report, string sortOrder)
    {
        report.DesignIssues = sortOrder switch
        {
            "severity-high-low" => report.DesignIssues.OrderByDescending(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
            "severity-low-high" => report.DesignIssues.OrderBy(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
            _ => report.DesignIssues.OrderBy(i => i.Category.Name).ThenByDescending(i => i.Severity).ToList()
        };

        report.AccessibilityIssues = sortOrder switch
        {
            "severity-high-low" => report.AccessibilityIssues.OrderByDescending(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
            "severity-low-high" => report.AccessibilityIssues.OrderBy(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
            _ => report.AccessibilityIssues.OrderBy(i => i.Category.Name).ThenByDescending(i => i.Severity).ToList()
        };
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
        // Retrieve the report with the specified ID, including related design and accessibility issues along with their categories
        var report = await _context.Reports
            .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
            .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(r => r.Id == id);

        // If the report is not found, return a 404 Not Found response
        if (report == null) return NotFound();

        // Store the current sort order in ViewBag to be used by partial views for rendering sorted data
        ViewBag.CurrentSort = sortOrder;

        // Sort the list of design issues based on the provided sort order
        report.DesignIssues = sortOrder switch
        {
            // Sort by severity in descending order (high to low)
            "severity-high-low" => report.DesignIssues.OrderByDescending(i => i.Severity).ToList(),
            // Sort by severity in ascending order (low to high)
            "severity-low-high" => report.DesignIssues.OrderBy(i => i.Severity).ToList(),
            // Default sorting by category name in ascending order
            _ => report.DesignIssues.OrderBy(i => i.Category.Name).ToList()
        };

        // Sort the list of accessibility issues based on the provided sort order
        report.AccessibilityIssues = sortOrder switch
        {
            // Sort by severity in descending order (high to low)
            "severity-high-low" => report.AccessibilityIssues.OrderByDescending(i => i.Severity).ToList(),
            // Sort by severity in ascending order (low to high)
            "severity-low-high" => report.AccessibilityIssues.OrderBy(i => i.Severity).ToList(),
            // Default sorting by category name in ascending order
            _ => report.AccessibilityIssues.OrderBy(i => i.Category.Name).ToList()
        };

        // Render the design issues partial view to HTML with the sorted design issues list
        var designHtml = await _viewRenderService.RenderViewToStringAsync(this, "_DesignIssuesPartial", report.DesignIssues);

        // Render the accessibility issues partial view to HTML with the sorted accessibility issues list
        var accessibilityHtml = await _viewRenderService.RenderViewToStringAsync(this, "_AccessibilityIssuesPartial", report.AccessibilityIssues);

        // Return the rendered partial views as a JSON object
        return Json(new { designHtml, accessibilityHtml });
    }
}
