/*using NUnit.Framework;
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

        [Then("they should be able to open the accessibility issues")]
        public void ThenTheyShouldBeAbleToViewTheAccessibilityIssues()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Find accessibility Issues
            var accessibilityIssues = wait.Until(driver => driver.FindElement(By.Id("Modal")));

            // Click accessibility issues
            accessibilityIssues.Click();

            // Find accessibility text
            var accessibilityText = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("designIssuesOverlay")));

            // Assert that accessibility text is display
            Assert.That(AccessibilityText.Displayed, Is.True, "Overlay should be visible");
        }

        [Then("they will not be able to sort")]
        public void ThenTheyWillNotBeAbleToSort()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Find sort element
            var sortSelect = wait.Until(driver => driver.FindElement(By.Id("sortSelect")));

            // Assert that select is inactive
            Assert.That(sortSelect);

        }

        [Given("they sort by severity")]
        public void GivenTheySortBySeverity()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));

            // Find sort element
            var sortSelect = wait.Until(driver => driver.FindElement(By.Id("sortSelect")));

            // Click sort element to open
            sortSelect.Click();

            // Find sort method 
            var severitySort = wait.Until(driver => driver.FindElement(By.Id("sortSelect")));

            // Click that method
            severitySort.Click();
        }

        [Then("they will be able to sort")]
        public void ThenTheyWillBeAbleToSort()
        {
            // wait
            // find sort element
            // assert that it is active
        }

        [Then("they should not see the modal pop up again")]
        public void GivenTheyShouldNotSeeTheModalPopUpAgain()
        {
            // wait
            // find modal
            // assert that modal is not visible
        }

        [Given("they click the summary button")]
        public void GivenTheyClickTheSummaryButton()
        {
            // wait 
            // find button
            // click up button
            // find modal
            // assert that modal is visible
        }
    }
}*/