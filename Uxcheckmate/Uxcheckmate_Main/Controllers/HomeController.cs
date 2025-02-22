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
    private readonly UxCheckmateDbContext _context;
    private readonly IOpenAiService _openAiService; 
    private readonly IPa11yService _pa11yService;

    public HomeController(ILogger<HomeController> logger, UxCheckmateDbContext dbContext, IOpenAiService openAiService, IPa11yService pa11yService)
    {
        _logger = logger;
        _context = dbContext;
        _openAiService = openAiService;
        _pa11yService = pa11yService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
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