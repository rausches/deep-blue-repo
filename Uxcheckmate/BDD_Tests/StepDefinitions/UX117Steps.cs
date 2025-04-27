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
    public class UX117Steps
    {
        private readonly IWebDriver _driver;
        private readonly ScenarioContext _scenarioContext;

        public UX117Steps(IWebDriver driver, ScenarioContext scenarioContext){
            _driver = driver;
            _scenarioContext = scenarioContext;
        }
        [Then("the user will see a modal containing the summary and mock up image")]
        public void ThenHeWillSeeAModalContainingSummaryAndMockUp()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Find the AI elements
            var modal = wait.Until(driver => driver.FindElement(By.Id("Modal")));
            var summary = wait.Until(driver => driver.FindElement(By.Id("Summary")));
            var image = wait.Until(driver => driver.FindElement(By.Id("Image")));

            // Assert they are displayed
            Assert.That(modal, !Is.Empty);
            Assert.That(summary, !Is.Empty);
            Assert.That(image.Displayed, Is.True);
        }

        [Then("the user clicks the let's begin button")]
        public void TheUserClicksTheLetsBeginButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Find and click the let's begin button
            var button = wait.Until(driver => driver.FindElement(By.Id("button")));
            button.Click();
        }

        [Then("the modal will close")]
        public void TheModalWillClose()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Find and click the let's begin button
            var modal = wait.Until(driver => driver.FindElement(By.Id("Modal")));
           
           Assert.That(modal.Displayed, Is.False);
        }
    }
}