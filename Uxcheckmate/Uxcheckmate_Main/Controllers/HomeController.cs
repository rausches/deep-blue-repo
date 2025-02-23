using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Diagnostics;

namespace Uxcheckmate_Main.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient; 
    private readonly UxCheckmateDbContext _context;
    private readonly IOpenAiService _openAiService; 
    private readonly IPa11yService _pa11yService;
    private readonly IReportService _reportService;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient, UxCheckmateDbContext dbContext, IOpenAiService openAiService, IPa11yService pa11yService, IReportService reportService)
    {
        _logger = logger;
        _httpClient = httpClient;
        _context = dbContext;
        _reportService = reportService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

 /*   [HttpPost]
    public async Task<IActionResult> Report(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            ModelState.AddModelError("url", "URL cannot be empty.");
            return View("Index");
        }

        // Call OpenAI service to analyze the design issues
        List<DesignIssue> designIssues = await _openAiService.AnalyzeWebsite(url) ?? new List<DesignIssue>();
        List<Pa11yIssue> accessibilityIssues = await _pa11yService.AnalyzeAndSaveAccessibilityReport(url) ?? new List<Pa11yIssue>();

        var model = Tuple.Create<IEnumerable<DesignIssue>, IEnumerable<Pa11yIssue>>(designIssues, accessibilityIssues);

        return View("Results", model);
    }*/

    [HttpPost]
    public async Task<IActionResult> Report(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            ModelState.AddModelError("url", "URL cannot be empty.");
            return View("Index");
        }

        try
        {
            var scanResults = await _reportService.GenerateReportAsync(url);
            return View("Results", scanResults);
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

    [HttpGet]
    public IActionResult Disclosure()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}