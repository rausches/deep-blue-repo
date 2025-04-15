// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Moq;
// using Reqnroll;
// using System.Security.Claims;
// using Uxcheckmate_Main.Controllers;
// using Microsoft.Extensions.Logging;
// using Uxcheckmate_Main.Services;
// using Uxcheckmate_Main.Models;
// using NUnit.Framework;
// using OpenQA.Selenium;

// namespace BDD_Tests.StepDefinitions
// {
//     [Binding]
//     public class UserDashSteps
//     {
//         private readonly IWebDriver driver;
//         private HomeController _controller;
//         private IActionResult _result;
//         public UserDashSteps(IWebDriver webDriver)
//         {
//             driver = webDriver;
//         }
//         [Given("user is logged in")]
//         public void GivenUserIsLoggedIn()
//         {
//             var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
//             {
//                 new Claim(ClaimTypes.Name, "Priya"),
//                 new Claim(ClaimTypes.NameIdentifier, "priUser"),
//                 new Claim(ClaimTypes.Role, "User")
//             }, "mock"));
//             var httpContext = new DefaultHttpContext { User = user };
//             var controllerContext = new ControllerContext { HttpContext = httpContext };
//             var logger = Mock.Of<ILogger<HomeController>>();
//             var httpClient = new HttpClient();
//             var db = Mock.Of<UxCheckmateDbContext>();
//             var openAi = Mock.Of<IOpenAiService>();
//             var axeCore = Mock.Of<IAxeCoreService>();
//             var report = Mock.Of<IReportService>();
//             var pdf = Mock.Of<PdfExportService>();
//             var shot = Mock.Of<IScreenshotService>();
//             var viewRenderer = Mock.Of<IViewRenderService>();
//             _controller = new HomeController(logger, httpClient, db, openAi, axeCore, report, pdf, shot, viewRenderer)
//             {
//                 ControllerContext = controllerContext
//             };
//         }
//         [When("they click user dash")]
//         public void WhenTheyClickUserDash()
//         {
//             _result = _controller.UserDash();
//         }
//         [Then("they should be in user dash page")]
//         public void ThenTheyShouldSeeALogout()
//         {
//             var viewResult = _result as ViewResult;
//             Assert.That(viewResult, Is.Not.Null);
//             Assert.That(viewResult.ViewName, Is.Null.Or.Empty);
//         }
//         [When("they click logout button")]
//         public void WhenTheyClickLogoutButton()
//         {
//             var user = new ClaimsPrincipal(new ClaimsIdentity()); // Removing user account
//             var httpContext = new DefaultHttpContext { User = user };
//             var controllerContext = new ControllerContext { HttpContext = httpContext };
//             _controller.ControllerContext = controllerContext;
//         }
//         [Then("they should be logged out")]
//         public void ThenTheyShouldBeLoggedOut()
//         {
//             var isAuthenticated = _controller.HttpContext.User.Identity?.IsAuthenticated ?? false;
//             Assert.That(isAuthenticated, Is.False, "User is still authenticated after logout.");
//         }
//         // Selenium Additions Below
//         [Then("they should see user dashboard")]
//         public void ThenTheyShouldBeLoggedIn()
//         {
//             var dashboardLink = driver.FindElement(By.LinkText("Dashboard"));
//             dashboardLink.Click();
//             string currentUrl = driver.Url;
//             Assert.That(currentUrl, Does.Contain("/Home/UserDash"), "User is not on the dashboard page.");
//         }
//         [When("they go to user dashboard")]
//         public void WhenTheyGoToUserDashboard()
//         {
//             var dashboardLink = driver.FindElement(By.LinkText("Dashboard"));
//             dashboardLink.Click();
//         }
//         [Then("they should see that report")]
//         public void ThenTheyShouldSeeThatReport()
//         {
//             Assert.That(driver.PageSource.Contains("Report ID"), Is.True, "Report ID was not found on the dashboard page.");
//         }
        
//     }
// }