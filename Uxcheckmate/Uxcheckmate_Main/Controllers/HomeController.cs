using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.DTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;


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
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;


    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, UxCheckmateDbContext dbContext, 
        IOpenAiService openAiService, IAxeCoreService axeCoreService, IReportService reportService, 
        PdfExportService pdfExportService, IScreenshotService screenshotService, IViewRenderService viewRenderService,IBackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory scopeFactory, IMemoryCache cache)
        
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = dbContext;
        _axeCoreService = axeCoreService;
        _reportService = reportService;
        _pdfExportService = pdfExportService;
        _screenshotService = screenshotService;
        _viewRenderService = viewRenderService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _scopeFactory = scopeFactory;
        _cache = cache;
    }


    // ============================================================================================================
    // Static Pages
    // ============================================================================================================
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    public IActionResult ErrorPage() => View("ErrorPage");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    // ============================================================================================================
    // Report Logic
    // ============================================================================================================
    [HttpPost]
    public async Task<IActionResult> Report(string url, string sortOrder = "category", bool isAjax = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
            {
                ModelState.AddModelError("url", "URL cannot be empty.");
                return View("Index");
            }
                // Normalize the URL *before* checking if it's reachable
                url = NormalizeUrl(url);

            if (!await IsUrlReachable(url))
            {
                TempData["UrlUnreachable"] = "The URL you entered seems incorrect or no longer exists. Please try again.";
                return RedirectToAction("Index");
            }

            var websiteScreenshot = await CaptureScreenshot(url);
            if (string.IsNullOrEmpty(websiteScreenshot ))
            {
                _logger.LogError("Failed to capture screenshot for URL: {Url}", url);
                ModelState.AddModelError("", "An error occurred while capturing the screenshot.");
                return View("Index");
            }
            TempData["WebsiteScreenshot"] = websiteScreenshot;

            // Check if the user is authenticated and get the user ID
            string? userId = null;
            bool isAdmin = false;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var roleClaims = User.FindAll(ClaimTypes.Role);
                    isAdmin = roleClaims.Any(c => c.Value == "Admin");
            }

            // Create and save the report record.
            var report = await CreateOrUpdateReport(url, cancellationToken);

            // Run accessibility and design analysis
            var accessibilityIssues = await _axeCoreService.AnalyzeAndSaveAccessibilityReport(report, cancellationToken);

            // Attach results, and set Processing status
            report.AccessibilityIssues = accessibilityIssues.ToList();
            report.Status = "Processing"; 
            await _context.SaveChangesAsync(cancellationToken);

            // Queue background design work
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    // Retrieve a scoped instance of the database context
                    var scopedDbContext = scope.ServiceProvider.GetRequiredService<UxCheckmateDbContext>();
                    
                    // Retrieve a scoped instance of the report service
                    var scopedReportService = scope.ServiceProvider.GetRequiredService<IReportService>();

                    // Generate the design issues
                    var designIssues = await scopedReportService.GenerateReportAsync(report, token);

                    // Retrieve the most up-to-date version of the report from the database
                    var freshReport = await scopedDbContext.Reports
                        .Include(r => r.AccessibilityIssues)
                        .Include(r => r.DesignIssues)
                        .FirstOrDefaultAsync(r => r.Id == report.Id, token);

                    if (freshReport != null)
                    {
                        // Update the report with the new design issues
                        freshReport.DesignIssues = designIssues.ToList();

                        // Set the report's status to "Completed"
                        freshReport.Status = "Completed";

                        // Copy the summary from the original report 
                        freshReport.Summary = report.Summary;

                        // Save all changes back to the database
                        await scopedDbContext.SaveChangesAsync(token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Background design analysis failed.");
                }
            });

            // Fetch the full report inclunding related issues and categories
        /* if (string.IsNullOrEmpty(userId)){
                report.AccessibilityIssues = accessibilityIssues.ToList();
                report.DesignIssues = designIssues.ToList();
                foreach (var issue in report.AccessibilityIssues){
                    issue.Category = await _context.AccessibilityCategories.FindAsync(issue.CategoryId);
                }
                foreach (var issue in report.DesignIssues){
                    issue.Category = await _context.DesignCategories.FindAsync(issue.CategoryId);
                }
            }
            // Fetch the report from the database to include related issues and categories
            Report fullReport;
            if (!string.IsNullOrEmpty(userId)){
            // Fetch the full report including related issues and categories
            fullReport = await _context.Reports
                .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
                .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
                .FirstOrDefaultAsync(r => r.Id == report.Id);
            }else{
                fullReport = report;
            }*/

            // Load fresh report for view
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

            // Sort the report issues based on the selected sort order
            SortReportIssues(fullReport, sortOrder);

            // Apply sorting based on the provided sort order
            ViewBag.CurrentSort = sortOrder;

            // Add to TempData for PDF Printing when not logged in
            StoreReportInTempData(report);
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
            _logger.LogError(ex, "Unhandled error during report generation for URL: {Url}", url);
            TempData["ScrapingError"] = "An unexpected error occurred during the scan. Please try again.";
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    public IActionResult Feedback()
    {
        return View();
    }

    [Authorize] // Have to be logged in/Authorized to access
    public async Task<IActionResult> UserDash()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reports = await _context.Reports
            .Where(r => r.UserID == userId)
            .Include(r => r.DesignIssues).ThenInclude(d => d.Category)
            .Include(r => r.AccessibilityIssues).ThenInclude(a => a.Category)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.Id)
            .ToListAsync();
        var reportDTOs = reports.Select(r => new ReportDTO
        {
            Id = r.Id,
            Url = r.Url,
            Date = r.Date,
            DesignIssues = r.DesignIssues.Select(d => new DesignIssueDTO
            {
                Message = d.Message,
                Severity = d.Severity,
                Category = d.Category?.Name
            }).ToList(),
            AccessibilityIssues = r.AccessibilityIssues.Select(a => new AccessibilityIssueDTO
            {
                Message = a.Message,
                Severity = a.Severity,
                WCAG = a.WCAG,
                Category = a.Category?.Name
            }).ToList()
        }).ToList();
        return View(reportDTOs);
    }

    [Authorize] // Need to be logged in to submit feedback
    public async Task<IActionResult> UserFeedback(){
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> SubmitFeedback(UserFeedback feedback)
    {
        if (string.IsNullOrWhiteSpace(feedback.Message)){
            ModelState.AddModelError("", "Feedback cannot be empty.");
            return View("Feedback");
        }
        var isValid = Regex.IsMatch(feedback.Message, @"^[a-zA-Z0-9\s.,?!'""\-\(\)\[\]@]*$");
        if (!isValid){
            ModelState.AddModelError("", "Feedback contains invalid characters.");
            return View("Feedback");
        }
        feedback.UserID = User.Identity?.Name; 
        feedback.DateSubmitted = DateTime.UtcNow;
        _context.UserFeedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Feedback submitted!";
        return RedirectToAction("Feedback");
    }


    [HttpGet]
    public async Task<IActionResult> DownloadReport(int id = 0)
    {
        Report report;

        if (id > 0){
            report = await _context.Reports
                .Include(r => r.AccessibilityIssues)
                .Include(r => r.DesignIssues)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound("Report not found.");
            }
        }else{
            // Check TempData
            if (!TempData.TryGetValue("Report", out var serializedReportObj) || serializedReportObj is not string serializedReport){
                return NotFound("Report Data Missing");
            }
            try{
                var reportDTO = JsonSerializer.Deserialize<ReportDTO>(serializedReport, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (reportDTO == null){
                    return BadRequest("Failed to parse unsaved report.");
                }
                // Map reportDTO to Report
                report = new Report
                {
                    Url = reportDTO.Url,
                    Date = reportDTO.Date,
                    AccessibilityIssues = reportDTO.AccessibilityIssues.Select(a => new AccessibilityIssue
                    {
                        Message = a.Message,
                        Severity = a.Severity,
                        WCAG = a.WCAG,
                        Category = new AccessibilityCategory { Name = a.Category }
                    }).ToList(),
                    DesignIssues = reportDTO.DesignIssues.Select(d => new DesignIssue
                    {
                        Message = d.Message,
                        Severity = d.Severity,
                        Category = new DesignCategory { Name = d.Category }
                    }).ToList()
                };
            }catch (Exception ex){
                _logger.LogError(ex, "Error deserializing report from TempData.");
                return BadRequest("Unable to process the report.");
            }
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
        return Json(new { designHtml, accessibilityHtml,
                status = report.Status, summary = report.Summary });
    }

    // ============================================================================================================
    // Private Helper Methods
    // ============================================================================================================
    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> DeleteReport(int reportId)
    {
        var report = await _context.Reports
            .Include(r => r.AccessibilityIssues)
            .Include(r => r.DesignIssues)
            .FirstOrDefaultAsync(r => r.Id == reportId);

        if (report == null)
        {
            return NotFound();
        }

        // Remove related issues and the report itself
        _context.AccessibilityIssues.RemoveRange(report.AccessibilityIssues);
        _context.DesignIssues.RemoveRange(report.DesignIssues);
        _context.Reports.Remove(report);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Report with ID {ReportId} deleted.", reportId);

        return Ok();
    }


    private string NormalizeUrl(string url)
    {
        url = url.Trim();

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://" + url;
        }

        return url.TrimEnd('/');
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
            string cacheKey = $"screenshot_{url.ToLowerInvariant()}";

            // Try get from cache
            if (_cache.TryGetValue(cacheKey, out string cachedScreenshot))
            {
                _logger.LogInformation("Using cached screenshot for {Url}.", url);
                return cachedScreenshot;
            }

            // Not cached → capture screenshot
            var screenshotOptions = new PageScreenshotOptions { FullPage = true };
            var screenshot = await _screenshotService.CaptureScreenshot(screenshotOptions, url);

            if (string.IsNullOrEmpty(screenshot))
            {
                _logger.LogError("Failed to capture screenshot for URL: {Url}", url);
                return null;
            }

            // Cache for 1 hour
            _cache.Set(cacheKey, screenshot, TimeSpan.FromHours(1));

            return screenshot;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while capturing the screenshot for URL: {Url}", url);
            return null;
        }
    }
    private async Task<Report> CreateOrUpdateReport(string url, CancellationToken cancellationToken = default)
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
        if (!string.IsNullOrEmpty(userId)){
            // Seeing if user already has a report under the url
            var existingReport = await _context.Reports.Include(r => r.AccessibilityIssues).Include(r => r.DesignIssues).FirstOrDefaultAsync(r => r.Url == url && r.UserID == userId);
            if (existingReport != null){
                // Deleting old report information [May decide to archive in later sprint]
                _context.AccessibilityIssues.RemoveRange(existingReport.AccessibilityIssues);
                _context.DesignIssues.RemoveRange(existingReport.DesignIssues);
                _context.Reports.Remove(existingReport);

              //  _context.Entry(existingReport).State = EntityState.Detached;
                _logger.LogInformation("Old report for user {UserId} and URL {Url} removed.", userId, url);
            }
        }
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New report saved to DB with ID: {ReportId}", report.Id);

        return report;
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

    private void StoreReportInTempData(Report report)
    {
        var tempReport = new ReportDTO
        {
            Url = report.Url,
            Date = report.Date,
            DesignIssues = report.DesignIssues.Select(d => new DesignIssueDTO
            {
                Message = d.Message,
                Severity = d.Severity,
                Category = d.Category?.Name
            }).ToList(),
            AccessibilityIssues = report.AccessibilityIssues.Select(a => new AccessibilityIssueDTO
            {
                Message = a.Message,
                Severity = a.Severity,
                WCAG = a.WCAG,
                Category = a.Category?.Name
            }).ToList()
        };
        TempData["Report"] = JsonSerializer.Serialize(tempReport);
    }
}