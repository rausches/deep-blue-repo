using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index(ReportUrl urlLink)
    {
        // need to send a confimation message that url has been received
        if (string.IsNullOrEmpty(urlLink.Url))
        {
            ViewBag.Message = "Url has not been received";
            return View("Index");
        }

        ViewBag.Message = "Url has been received";
        return View("Index");
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
