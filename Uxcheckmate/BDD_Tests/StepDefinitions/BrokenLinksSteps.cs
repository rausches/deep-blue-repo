using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Reqnroll;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Services;

namespace BDD_Tests.StepDefinitions
{
    [Binding]
    public class BrokenLinksSteps
    {
        private readonly IWebDriver _driver;
        private readonly ScenarioContext _scenarioContext;
        private readonly string _url = "https://momkage-lexy.github.io/";
        private readonly WebScraperService _scraperService;


        private readonly string _html;

        public BrokenLinksSteps(IWebDriver driver, ScenarioContext scenarioContext)
        {
            _driver = driver;
            _scenarioContext = scenarioContext;
            _scenarioContext["skipServer"] = true;

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<WebScraperService>();
            _scraperService = new WebScraperService(new HttpClient(), logger);

            _driver.Navigate().GoToUrl(_url);
            _html = _driver.PageSource;
            _driver.Navigate().GoToUrl("http://localhost:5000");
        }

        [Given("the user has generated a report for \"(.*)\"")]
        public void GivenTheUserHasGeneratedAReport(string siteUrl)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));
            var input = wait.Until(d => d.FindElement(By.Id("urlInput")));
            input.Clear();
            input.SendKeys(siteUrl);

            var button = _driver.FindElement(By.Id("analyzeBtn"));
            button.Click();

            wait.Until(d => d.FindElement(By.Id("reportContainer"))); // Wait for report
        }

        [When("the user clicks the broken links section")]
        public void ThenHeClicksTheBrokenLinksSection()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Find and click the accordion button that opens the Broken Links section
            var button = wait.Until(driver => driver.FindElement(By.XPath("//button[contains(., 'Broken Links')]")));
            button.Click();
        }

        [Then("the broken links section should be visible")]
        public void ThenTheBrokenLinksSectionShouldBeVisible()
        {
            var section = _driver.FindElement(By.XPath("//button[contains(., 'Broken Links')]"));
            Assert.That(section.Displayed, Is.True);
                        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        [Then("the broken links row reports missing or invalid links")]
        public async Task ThenTheBrokenLinksRowReportsMissingOrInvalidLinks()
        {
            /*==============================================================================
                                        Backend to UI Testing

            Check: What is expected based on service calls is what is displayed on the UI
            ==============================================================================*/
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            
            // Call web scraper to organize html elements
            var scrapedData = _scraperService.ExtractHtmlElements(_html, _url);

            // Create Broken Links Service Instance
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BrokenLinksService>();
            var service = new BrokenLinksService(new HttpClient(), logger);

            // Call service method to check for broken links
            string expectedReport = await service.BrokenLinkAnalysis(_url, scrapedData);

            // Wait until the element with ID "broken-links" is both visible and has non-empty text
            var content = wait.Until(driver =>
            {
                // Attempt to find the element in the DOM
                var el = driver.FindElement(By.Id("broken-links"));

                // Only return the element if it is displayed AND contains some text
                return el.Displayed && !string.IsNullOrWhiteSpace(el.Text) ? el : null;
            });

            string actualReport = content.Text;
            Console.WriteLine(actualReport);

            // Assert that both reports match
            if (scrapedData.TryGetValue("links", out var linkObj) && linkObj is List<string> links)
            {
                foreach (var link in links)
                {
                    if (expectedReport.Contains(link))
                    {
                        Assert.That(actualReport, Does.Contain(link), $"Expected UI to contain broken link: {link}");
                    }
                }
            }
            else
            {
                Assert.Fail("No links found in scraped data to validate.");
            }
        }
    }
}

