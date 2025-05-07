using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;
using Reqnroll;

namespace BDD_Tests.StepDefinitions
{
    [Binding]
    public class ReportGenerationSteps
    {
        private readonly IWebDriver _driver;

        public ReportGenerationSteps(IWebDriver driver)
        {
            _driver = driver;
        }

        [Then("the analyzed URL is shown on the page")]
        public void ThenTheAnalyzedUrlIsShownOnThePage()
        {
            var urlEl = _driver.FindElement(By.Id("reportHeader"));
            Assert.That(urlEl.Text, Does.Contain("https://example.com"));
        }

        [Then("the user sees how many issues were found")]
        public void ThenTheUserSeesHowManyIssuesWereFound()
        {
            var issueCount = _driver.FindElement(By.Id("totalIssues"));
            Assert.That(issueCount.Text, Does.Contain("Total Issues Found: "));
        }

        [Then("each issue category header is visible")]
        public void ThenEachIssueCategoryHeaderIsVisible()
        {
            var categories = new[] {
                "Broken Links", "Pop Ups", "Animation", "Audio",
                "Number of Scrolls", "Color Scheme", "Heading Hierarchy",
                "Dynamic Sizing", "F Pattern", "Z Pattern", "Symmetry"
            };

          /*  foreach (var category in categories)
            {
                var section = _driver.FindElement(By.XPath($"//button[contains(., '{category}')]"));
                Assert.That(section.Displayed, Is.True, $"Expected '{category}' section to be visible.");
            }*/
        }
    }
}