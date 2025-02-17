using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services; 

namespace Uxcheckmate_Main.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IOpenAiService _openAiService; 

    public HomeController(ILogger<HomeController> logger, IOpenAiService openAiService)
    {
        _logger = logger;
        _openAiService = openAiService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(ReportUrl model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Call OpenAiApiService
        var reports = await _openAiService.AnalyzeAndSaveUxReports(model.Url);
       
        // Pass the reports to a results view 
        return View("Results", reports);
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

   /* [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }*/
}
