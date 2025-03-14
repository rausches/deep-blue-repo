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
            // Create and save the initial report record
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

            // Run accessibility and design analysis
            await _axeCoreService.AnalyzeAndSaveAccessibilityReport(report);
            await _reportService.GenerateReportAsync(report);

            // Fetch the full report including related issues and categories
            var fullReport = await _context.Reports
                .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
                .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            // Handle the case where the report could not be fetched
            if (fullReport == null)
            {
                _logger.LogError("Failed to fetch report with ID: {ReportId}", report.Id);
                ModelState.AddModelError("", "An error occurred while fetching the report.");
                return View("Index");
            }

            // Apply sorting based on the provided sort order
            ViewBag.CurrentSort = sortOrder;

            // Sort design issues
            fullReport.DesignIssues = sortOrder switch
            {
                "severity-high-low" => fullReport.DesignIssues.OrderByDescending(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
                "severity-low-high" => fullReport.DesignIssues.OrderBy(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
                _ => fullReport.DesignIssues.OrderBy(i => i.Category.Name).ThenByDescending(i => i.Severity).ToList()
            };

            // Sort accessibility issues
            fullReport.AccessibilityIssues = sortOrder switch
            {
                "severity-high-low" => fullReport.AccessibilityIssues.OrderByDescending(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
                "severity-low-high" => fullReport.AccessibilityIssues.OrderBy(i => i.Severity).ThenBy(i => i.Category.Name).ToList(),
                _ => fullReport.AccessibilityIssues.OrderBy(i => i.Category.Name).ThenByDescending(i => i.Severity).ToList()
            };

            // If the request is an AJAX call, return the partial view
            if (isAjax)
            {
                return PartialView("_ReportSections", fullReport);
            }

            // Return the full results view
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
        var designHtml = await this.RenderViewAsync("_DesignIssuesPartial", report.DesignIssues);

        // Render the accessibility issues partial view to HTML with the sorted accessibility issues list
        var accessibilityHtml = await this.RenderViewAsync("_AccessibilityIssuesPartial", report.AccessibilityIssues);

        // Return the rendered partial views as a JSON object
        return Json(new { designHtml, accessibilityHtml });
    }
}

public static class ControllerExtensions
{
    public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model)
    {
        // Assign the model to the ViewData to ensure the view has access to it.
        controller.ViewData.Model = model;

        // Create a StringWriter to capture the rendered HTML output.
        using var writer = new StringWriter();

        // Retrieve the view engine service from the dependency injection container.
        var viewEngine = controller.HttpContext.RequestServices.GetService<ICompositeViewEngine>();

        // Attempt to find the specified view using the view engine.
        var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, false);

        // If the view cannot be found, throw an exception.
        if (!viewResult.Success)
            throw new Exception($"View '{viewName}' not found");

        // Create a ViewContext, which encapsulates all information required for rendering.
        var viewContext = new ViewContext(
            controller.ControllerContext, // Current controller context
            viewResult.View,              // The view to render
            controller.ViewData,          // ViewData containing the model
            controller.TempData,          // Temporary data storage
            writer,                       // Output writer to capture the rendered view
            new HtmlHelperOptions()       // Helper options for rendering
        );

        // Render the view asynchronously into the StringWriter.
        await viewResult.View.RenderAsync(viewContext);

        // Return the rendered content as a string.
        return writer.ToString();
    }
}
