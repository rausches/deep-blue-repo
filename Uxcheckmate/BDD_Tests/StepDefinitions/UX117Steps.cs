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
        [Then("the user will see a modal containing the summary")]
        public void ThenHeWillSeeAModalContainingSummary()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));
    
            // Wait for modal to be visible
            var modal = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("onLoadModal")));

            // Wait specifically for the summary paragraph inside the modal
            var summary = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("summary")));

            Assert.That(summary.Displayed, Is.True, "Summary should be visible inside modal.");
        }

        [Then("the user clicks the let's begin button")]
        public void TheUserClicksTheLetsBeginButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(300));
            var button = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("letsgo")));
            // Find and click the let's begin button
          //  var button = wait.Until(driver => driver.FindElement(By.Id("letsgo")));
            button.Click();
        }

        [Then("the modal will close")]
        public void TheModalWillClose()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            
            bool isInvisible = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(By.Id("onLoadModal")));
            
            Assert.That(isInvisible, Is.True, "Modal should be closed.");
        }
    }
}