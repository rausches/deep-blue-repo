using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
public static class TestBuilder
{
    public static HomeController BuildHomeController(HttpContext httpContext, UxCheckmateDbContext context)
    {
        return new HomeController(
            new LoggerFactory().CreateLogger<HomeController>(),
            new HttpClient(),
            context,
            new Mock<IOpenAiService>().Object,
            new Mock<IAxeCoreService>().Object,
            new ReportService(
                new HttpClient(),
                new LoggerFactory().CreateLogger<ReportService>(),
                context,
                new Mock<IOpenAiService>().Object,
                new Mock<IBrokenLinksService>().Object,
                new Mock<IHeadingHierarchyService>().Object,
                new Mock<IColorSchemeService>().Object,
                new Mock<IDynamicSizingService>().Object,
                new Mock<IScreenshotService>().Object,
                new Mock<IWebScraperService>().Object
            ),
            new Mock<PdfExportService>().Object,
            new Mock<IScreenshotService>().Object,
            new Mock<IViewRenderService>().Object
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
}