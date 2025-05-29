using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using SeleniumExtras.WaitHelpers;

namespace BDD_Tests.StepDefinitions
{
    [Binding]
    public class UX123Steps
    {
        private readonly IWebDriver _driver;
        private readonly ScenarioContext _scenarioContext;

        public UX123Steps(IWebDriver driver, ScenarioContext scenarioContext){
            _driver = driver;
            _scenarioContext = scenarioContext;
        }

        [Given("the design issues are still loading")]
        public void GivenTheDesignIssuesAreStillLoading()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Wait for design issues overlay to be visible
            var loadingOverlay = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("designIssuesOverlay")));

            // Assert that overlay is visible
            Assert.That(loadingOverlay.Displayed, Is.True, "Overlay should be visible");
        }

        [Then("they should be able to view the accessibility issues")]
        public void ThenTheyShouldBeAbleToViewTheAccessibilityIssues()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(100));
            var js = (IJavaScriptExecutor)_driver;

            string targetId = "collapse-accessibility-other"; // or whatever ID you're testing
            var toggleButton = _driver.FindElement(By.CssSelector($"button[data-bs-target='#{targetId}']"));

            // Scroll into view just in case
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", toggleButton);

            // Use Bootstrap collapse API via JavaScript to open it
            js.ExecuteScript(@"
                var target = document.getElementById(arguments[0]);
                var bsCollapse = bootstrap.Collapse.getOrCreateInstance(target);
                bsCollapse.show();
            ", targetId);

            // Wait for the panel to be open
            var openPanel = wait.Until(driver =>
            {
                var panel = driver.FindElement(By.Id(targetId));
                var classAttr = panel.GetAttribute("class");
                return classAttr.Contains("show") ? panel : null;
            });

            // Confirm issueDetails inside the open panel
            var issueDetail = openPanel.FindElement(By.ClassName("issueDetails"));
            Assert.That(issueDetail.Displayed, Is.True, "Expected issue details to be visible inside the open accordion.");
        }

        [Then("they will not be able to sort")]
        public void ThenTheyWillNotBeAbleToSort()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Find sort element
            var sortSelect = wait.Until(driver => driver.FindElement(By.Id("sortSelect")));

            Assert.That(sortSelect.Enabled, Is.False, "Select element should be disabled.");
        }

        [Then("they sort by severity")]
        public void ThenTheySortBySeverity()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Open dropdown options and select sort
            var sortSelect = new SelectElement(_driver.FindElement(By.Id("sortSelect")));
            sortSelect.SelectByText("Severity (High to Low)");
        }

        [Then("they should not see the modal pop up again")]
        public void GivenTheyShouldNotSeeTheModalPopUpAgain()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));
            // find modal
            var summary = wait.Until(driver => driver.FindElement(By.Id("onLoadModal")));

            // assert that modal is not visible
            Assert.That(summary.Displayed, Is.False, "Modal should not be visible");
        }

        [Then("the user clicks the summary button")]
        public void ThenTheUserClicksTheSummaryButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // find button
            var summaryBtn = wait.Until(driver => driver.FindElement(By.Id("viewSummaryBtn")));
            
            // click up button
            summaryBtn.Click();
        }
    }
}