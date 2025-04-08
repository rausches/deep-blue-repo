using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate.BDD_Tests.StepDefinitions
{
    [Binding]
    public class AnalyzeSiteSteps
    {
        private readonly IWebDriver _driver;
        private readonly IReportService _reportService;
        private readonly IWebScraperService _scraperService;
        private readonly ScenarioContext _scenarioContext;
        private readonly string _scannedUrl = "https://momkage-lexy.github.io/";
        private string _scannedHtml;

        public AnalyzeSiteSteps(IWebDriver driver, ScenarioContext scenarioContext)
        {
            _driver = driver;
            _scenarioContext = scenarioContext;

            var reportService = new ReportService(
            new HttpClient(),
            new LoggerFactory().CreateLogger<ReportService>(),
            new Mock<UxCheckmateDbContext>().Object,
            new Mock<IOpenAiService>().Object,
            new Mock<IBrokenLinksService>().Object,
            new Mock<IHeadingHierarchyService>().Object,
            new Mock<IColorSchemeService>().Object,
            new Mock<IDynamicSizingService>().Object,
            new Mock<IScreenshotService>().Object,
            new Mock<IWebScraperService>().Object
            );

            _reportService = reportService;
            _scraperService = new WebScraperService(
                new HttpClient(),
                new LoggerFactory().CreateLogger<WebScraperService>()
            );

            _driver.Navigate().GoToUrl(_scannedUrl);
            _scannedHtml = _driver.PageSource;
        }

        [Given("David is on the homepage")]
        public void GivenDavidIsOnTheHomepage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000");
        }

        [Given(@"he enters ""(.*)"" into the submission box")]
        public void GivenHeEntersHisURLIntoTheSubmissionBox(string url)
        {
            _driver.FindElement(By.Id("urlInput")).SendKeys(url);
        }

        [Given("he clicks the analyze button")]
        public void GivenHeClicksTheAnalyzeButton()
        {
            _driver.FindElement(By.Id("analyzeBtn")).Click();
        }

        [Then("he will see a loading overlay")]
        public void ThenHeWillSeeALoadingOverlay()
        {
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => driver.FindElement(By.Id("scanningWrapper")));
        }

        [Then("he will be directed to the results view")]
        public void ThenHeWillBeDirectedToTheResultsView()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));
            var resultsView = wait.Until(driver => driver.FindElement(By.Id("reportContainer")));
            Assert.That(resultsView.Displayed, "Report is not displayed");
        }

        [Then("he will see a screenshot of the site")]
        public void ThenHeWillSeeAScreenshotOfTheSite()
        {
            // Assert screenshot element is present (placeholder)
        }

        [Then("he will see the site URL")]
        public void ThenHeWillSeeTheSiteURL()
        {
            string expectedUrl = "https://momkage-lexy.github.io/";
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            IWebElement urlHeader = wait.Until(driver => driver.FindElement(By.Id("reportHeader")));
            string actualUrl = urlHeader.Text;
            Assert.That(urlHeader.Displayed, "URL is not displayed.");
            Assert.That(expectedUrl, Is.EqualTo(actualUrl), $"Expected site URL to be '{expectedUrl}' but was '{actualUrl}'.");
        }

        [Then("he will see how many issues his site has")]
        public void ThenHeWillSeeHowManyIssuesHisSiteHas()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var issueCountElement = wait.Until(driver => driver.FindElement(By.Id("totalIssues")));
            string issueText = issueCountElement.Text;
            Assert.That(issueCountElement.Displayed, "Issue count is not displayed.");
            Assert.That(issueText, Does.Contain("Total Issues Found:"), "Issue count text not found or incorrect.");
        }

        [Then("he will see a container for design issues with subrows of issues")]
        public void ThenHeWillSeeAContainerForDesignIssues()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var designContainer = wait.Until(driver => driver.FindElement(By.Id("designIssuesAccordion")));
            var designIssues = designContainer.FindElements(By.Id("designIssueItem"));
            Assert.That(designContainer.Displayed, "Design container is not displayed.");
            Assert.That(designIssues.Count > 0, "No design issues were found in the container.");
        }

        [Then("he will see a container for accessibility issues with subrows of issues")]
        public void ThenHeWillSeeAContainerForAccessibilityIssues()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var accessibilityContainer = wait.Until(driver => driver.FindElement(By.Id("accessibilityIssuesAccordion")));
            var accessibilityIssues = accessibilityContainer.FindElements(By.Id("accessibilityIssueItem"));
            Assert.That(accessibilityContainer.Displayed, "Accessibility container is not displayed.");
            Assert.That(accessibilityIssues.Count > 0, "No accessibility issues were found in the container.");
        }

        [Then("the broken links analysis should report missing or invalid links")]
        public async Task ThenTheBrokenLinksAnalysisShouldReportMissingOrInvalidLinks()
        {
            
            /*=====================================================
                            Backend to UI Testing
            =====================================================*/
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            // Call web scraper to organize html elements
            var scrapedData = _scraperService.ExtractHtmlElements(_scannedHtml, _scannedUrl);

            // Create Broken Links Service Instance
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<BrokenLinksService>();
            var httpClient = new HttpClient();
            var brokenLinkService = new BrokenLinksService(httpClient, logger);

            // Call service method to check for broken links
            string expectedReport = await brokenLinkService.BrokenLinkAnalysis(_scannedUrl, scrapedData);

            // UI interaction to get broken links report
            var button = wait.Until(driver => driver.FindElement(By.XPath("//button[contains(., 'Broken Links')]")));
            button.Click();

            var contentElement = wait.Until(driver => driver.FindElement(By.Id("broken-links")));
            string actualReport = contentElement.Text;

            // Assert that both reports match
            if (scrapedData.TryGetValue("links", out var linkObj) && linkObj is List<string> links)
            {
                foreach (var link in links)
                {
                    if (expectedReport.Contains(link))
                    {
                        Console.WriteLine($"🔍 Verifying that UI report contains: {link}");
                        Assert.That(actualReport, Does.Contain(link), $"Expected UI to contain broken link: {link}");
                    }
                }
            }
            else
            {
                Assert.Fail("No links found in scraped data to validate.");
            }
        }

      /*  [Then("the heading hierarchy analysis should detect outline problems")]
        public void ThenTheHeadingHierarchyAnalysisShouldDetectOutlineProblems()
        {
            _scenarioContext.Pending();
        }

        [Then("the font legibility analysis should detect {string} as illegible")]
        public void ThenTheFontLegibilityAnalysisShouldDetectAsIllegible(string font)
        {
            _scenarioContext.Pending();
        }

        [Then("the color scheme analysis should return a suggestion")]
        public void ThenTheColorSchemeAnalysisShouldReturnASuggestion()
        {
            _scenarioContext.Pending();
        }

        [Then("the favicon analysis should recommend adding a favicon")]
        public void ThenTheFaviconAnalysisShouldRecommendAddingAFavicon()
        {
            _scenarioContext.Pending();
        }

        [Then("the dynamic sizing analysis should return a recommendation")]
        public void ThenTheDynamicSizingAnalysisShouldReturnARecommendation()
        {
            _scenarioContext.Pending();
        }*/

    }
}
