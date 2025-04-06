using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;

namespace Uxcheckmate.BDD_Tests.StepDefinitions
{
    [Binding]
    public class AnalyzeSiteSteps
    {
        private readonly IWebDriver _driver;

        public AnalyzeSiteSteps(IWebDriver driver)
        {
            _driver = driver;
        }

        [Given("David is on the homepage")]
        public void GivenDavidIsOnTheHomepage()
        {
            _driver.Navigate().GoToUrl("https://localhost:5000");
        }

        [Given("he enters his URL into the submission box")]
        public void GivenHeEntersHisURLIntoTheSubmissionBox()
        {
            _driver.FindElement(By.Id("urlInput")).SendKeys("https://example.com");
        }

        [Given("he clicks the analyze button")]
        public void GivenHeClicksTheAnalyzeButton()
        {
            _driver.FindElement(By.Id("analyzeBtn")).Click();
        }

        [Then("he will see a loading overlay")]
        public void ThenHeWillSeeALoadingOverlay()
        {
            // 10s Timer
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait until overlay appears
            IWebElement overlay = wait.Until(driver => driver.FindElement(By.Id("scanningWrapper")));

            // Exception if not displayed
            if (!overlay.Displayed)
            {
                throw new Exception("Loading overlay is not displayed.");
            }
        }

        [Then("he will be directed to the results view")]
        public void ThenHeWillBeDirectedToTheResultsView()
        {
            // 180s Timer
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            // Wait until report container is found
            IWebElement resultsView = wait.Until(driver => driver.FindElement(By.Id("reportContainer")));

            // Exception if not displayed
            if (!resultsView.Displayed)
            {
                throw new Exception("Report is not displayed");
            }

        }

        [Then("he will see a screenshot of the site")]
        public void ThenHeWillSeeAScreenshotOfTheSite()
        {
            // Assert screenshot element is present
        }

        [Then("he will see the site URL")]
        public void ThenHeWillSeeTheSiteURL()
        {
            string expectedUrl = "https://example.com";

            // 10s Timer
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait until report header is found
            IWebElement urlHeader = wait.Until(driver => driver.FindElement(By.Id("reportHeader")));

            string actualUrl = urlHeader.Text;

            // Exception if not displayed
            if (!urlHeader.Displayed)
            {
                throw new Exception("URL is not displayed.");
            }

            // Nunit Assertion
            Assert.That(expectedUrl, Is.EqualTo(actualUrl), $"Expected site URL to be '{expectedUrl}' but was '{actualUrl}'.");
        }

        [Then("he will see how many issues his site has")]
        public void ThenHeWillSeeHowManyIssuesHisSiteHas()
        {
            // 10s Timer
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait until total issues is found
            var issueCountElement = wait.Until(driver => driver.FindElement(By.Id("totalIssues")));

            string issueText = issueCountElement.Text;

            // Exception if not displayed
            if (!issueCountElement.Displayed)
            {
                throw new Exception("Issue count is not displayed.");
            }

            // Nunit assertion
            Assert.That(issueText, Does.Contain("Total Issues Found:"), "Issue count text not found or incorrect.");
        }

        [Then("he will see a container for design issues with subrows of issues")]
        public void ThenHeWillSeeAContainerForDesignIssues()
        {
            // 10s Timer
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait until design container found
            var designContainer = wait.Until(driver => driver.FindElement(By.Id("designIssuesAccordion")));

            // Find issues subrow
            var designIssues = designContainer.FindElements(By.Id("designIssueItem"));

            // Exception if not displayed
            if (!designContainer.Displayed)
            {
                throw new Exception("Design container is not displayed.");
            }

            // Nunit Assertion
            Assert.That(designIssues.Count > 0, "No design issues were found in the container.");
        }

        [Then("he will see a container for accessibility issues with subrows of issues")]
        public void ThenHeWillSeeAContainerForAccessibilityIssues()
        {
            // 10s Timer
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait until accessibility container found
            var accessibilityContainer = wait.Until(driver => driver.FindElement(By.Id("accessibilityIssuesAccordion")));

            // Find issues subrow
            var accessibilityIssues = accessibilityContainer.FindElements(By.Id("accessibilityIssueItem"));

            // Exception if not displayed
            if (!accessibilityContainer.Displayed)
            {
                throw new Exception("Accessibility container is not displayed.");
            }

            // Nunit Assertion
            Assert.That(accessibilityIssues.Count > 0, "No accessibility issues were found in the container.");
        }
    }
}
