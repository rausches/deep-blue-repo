using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Net.Http;
using System.Security.Claims;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;


namespace Moq_Tests;

public static class TestBuilder
{
    public static HomeController BuildHomeController(HttpContext httpContext, UxCheckmateDbContext context)
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Captcha:SecretKey"]).Returns("test-secret");
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
                new Mock<IMobileResponsivenessService>().Object,
                new Mock<IScreenshotService>().Object,
                new Mock<IPlaywrightScraperService>().Object,
                new Mock<IPopUpsService>().Object,
                new Mock<IAnimationService>().Object,
                new Mock<IAudioService>().Object,
                new Mock<IScrollService>().Object,
                new Mock<IFPatternService>().Object,
                new Mock<IZPatternService>().Object,
                new Mock<ISymmetryService>().Object,
                new Mock<IServiceScopeFactory>().Object,
                new Mock<IMemoryCache>().Object
            ),
            new Mock<PdfExportService>().Object,
            new Mock<IScreenshotService>().Object,
            new Mock<IViewRenderService>().Object,
            new Mock<IBackgroundTaskQueue>().Object,
            new Mock<IServiceScopeFactory>().Object,
            new Mock<IMemoryCache>().Object,
            BuildUserManager(),
            configMock.Object
        )
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };
    }
    public static UserManager<IdentityUser> BuildUserManager()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<IdentityUser>>();
        var userValidators = new List<IUserValidator<IdentityUser>> { new Mock<IUserValidator<IdentityUser>>().Object };
        var passwordValidators = new List<IPasswordValidator<IdentityUser>> { new Mock<IPasswordValidator<IdentityUser>>().Object };
        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new Mock<IdentityErrorDescriber>();
        var serviceProvider = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<IdentityUser>>>();
        var userManager = new UserManager<IdentityUser>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors.Object,
            serviceProvider.Object,
            logger.Object
        );
        store.Setup(s => s.FindByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(new IdentityUser("testUser"));
        return userManager;
    }

}


// TestHtmlElement is a mock class for testing purposes
public class TestHtmlElement : HtmlElement
{
    public TestHtmlElement(double x, double y, double width, double height, double desiredDensity)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        if (Area > 0){
            int textLength = (int)(desiredDensity * Area);
            Text = new string('A', Math.Max(textLength, 1)); // Fake character using different amount of As
        }else{
            Text = "";
        }
    }
}