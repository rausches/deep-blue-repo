using System.IO;
using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;

namespace Database_Tests
{
    public class Database_Tests
    {
        private UxCheckmateDbContext _context;
        private DbContextOptions<UxCheckmateDbContext> _options;
        [SetUp]
        public void Setup()
        {
            var configPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "appsettings.json");
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.WorkDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var dbConnectionString = configuration.GetConnectionString("DBConnection");

            // Debugging output
            Console.WriteLine($"Retrieved Connection String: {dbConnectionString}");

            if (string.IsNullOrEmpty(dbConnectionString))
            {
                throw new InvalidOperationException("DBConnection was not found in appsettings.json!");
            }

            _options = new DbContextOptionsBuilder<UxCheckmateDbContext>().UseSqlServer(dbConnectionString).Options;
            _context = new UxCheckmateDbContext(_options);

        }

        [Test]
        public void Database_Connected()
        {
            bool canConnect = _context.Database.CanConnect();
            Assert.That(canConnect, Is.True);
        }

        [Test]
        public void Read_Categories()
        {
            var DesignCategory = _context.DesignCategories.ToList();
            Assert.That(DesignCategory, Is.Not.Empty);
            Assert.That(DesignCategory.Any(d => d.Id == 18 && d.Name == "Visual Hierarchy"), Is.True);
            Assert.That(DesignCategory.Any(d => d.Id == 19 && d.Name == "Broken Links"), Is.True);
        }

        [Test]
        public void Add_Test_Issue_Test()
        {
            var category = new DesignIssue { CategoryId = 18, ReportId = 1, Message = "Test Message", Severity = 1 };
            _context.DesignIssues.Add(category);
            _context.SaveChanges();
            var testCategory = _context.DesignIssues.FirstOrDefault(c => c.Message == "Test Message");
            Assert.That(testCategory, Is.Not.Null);
            Assert.That(testCategory.CategoryId, Is.EqualTo(18));
            Assert.That(testCategory.ReportId, Is.EqualTo(1));
            Assert.That(testCategory.Severity, Is.EqualTo(1));
        }

        // Singular URL Tests
        [Test]
        public async Task DeleteOldReportAndMakeNewOne()
        {
            var userId = "testUserID";
            var url = "https://example.com/";
            // Creating a old report
            var oldReport = new Report
            {
                Url = url,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                UserID = userId
            };
            _context.Reports.Add(oldReport);
            await _context.SaveChangesAsync();
            // Setup mock authenticated user
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "TestAuth"));
            // Creating a controller to run new report
            var controller = new HomeController(
                new LoggerFactory().CreateLogger<HomeController>(),
                new HttpClient(),
                _context,
                new Mock<IOpenAiService>().Object,
                new Mock<IAxeCoreService>().Object,
                new ReportService(
                    new HttpClient(),
                    new LoggerFactory().CreateLogger<ReportService>(),
                    _context,
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
            // Run report generation to replace old report
            await controller.Report(url);
            var newReport = await _context.Reports.FirstOrDefaultAsync(r => r.Url == url && r.UserID == userId);
            Assert.That(newReport, Is.Not.Null, "New report should exist.");
            Assert.That(await _context.Reports.CountAsync(r => r.Url == url && r.UserID == userId), Is.EqualTo(1), "Only one report should exist for this user and URL.");
            Assert.That(newReport.Date, Is.GreaterThan(oldReport.Date), "New report should be newer than the old one.");
        }
        [Test]
        public async Task OtherUsersReportNotDeleted()
        {
            var url = "https://example.com/";
            var userID1 = "testUserID1";
            var userID2 = "testUserID2";
            // Creating a intial report for each user
            var initialDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
            _context.Reports.AddRange(
                new Report { Url = url, UserID = userID1, Date = initialDate },
                new Report { Url = url, UserID = userID2, Date = initialDate }
            );
            await _context.SaveChangesAsync();
            // Setup mock userID1
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userID1)
            }, "TestAuth"));
            var controller = new HomeController(
                new LoggerFactory().CreateLogger<HomeController>(),
                new HttpClient(),
                _context,
                new Mock<IOpenAiService>().Object,
                new Mock<IAxeCoreService>().Object,
                new ReportService(
                    new HttpClient(),
                    new LoggerFactory().CreateLogger<ReportService>(),
                    _context,
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
            // Running report [Should replace old report]
            await controller.Report(url);
            // Couting only reports associated with the users
            var reports = await _context.Reports.Where(r => r.Url == url && (r.UserID == userID1 || r.UserID == userID2)).ToListAsync();
            Assert.That(reports.Count, Is.EqualTo(2), "Both users should still have reports.");
            Assert.That(reports.Any(r => r.UserID == userID2), Is.True, "User2's report should not have been deleted.");
            var user1Report = reports.FirstOrDefault(r => r.UserID == userID1);
            Assert.That(user1Report, Is.Not.Null);
            Assert.That(user1Report.Date, Is.GreaterThan(initialDate), "User1's report should have been replaced with a newer one.");
        }
        [TearDown]
        public void TearDown()
        {
            // Cleaning the url test data
            var testUserIds = new[] { "testUserID", "testUserID1", "testUserID2" };
            var testReports = _context.Reports.Where(r => testUserIds.Contains(r.UserID));
            _context.Reports.RemoveRange(testReports);

            var testCategories = _context.DesignIssues.Where(c => c.Message == "Test Message");
            _context.DesignIssues.RemoveRange(testCategories);
            _context.SaveChanges();
            _context.Dispose();
        }
    }
}