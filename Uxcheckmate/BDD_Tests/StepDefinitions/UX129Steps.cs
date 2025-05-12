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
    public class UX129Steps
    {
        private readonly IWebDriver _driver;
        private readonly ScenarioContext _scenarioContext;

        public UX129Steps(IWebDriver driver, ScenarioContext scenarioContext){
            _driver = driver;
            _scenarioContext = scenarioContext;
        }

        [Given("the user navigates to the login page")]
        public void GivenTheUserNavigatesToTheLoginPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:5000/Identity/Account/Login");
        }

        [Given("the user is on the dashboard")]
        public void AndTheUserIsOnTheDashboard()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            // Find and click the dashboard link
            var dashboardLink = wait.Until(driver => driver.FindElement(By.Id("userDash")));
            dashboardLink.Click();
        }

        [When("the user clicks on a domain entry")]
        public void WhenUserClicksOnDomainEntry()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Click the first accordion toggle (or find by domain if you want specific one)
            var accordionToggle = wait.Until(d => d.FindElement(By.CssSelector(".accordion-button.collapsed")));
            accordionToggle.Click();

            // Wait for the collapse panel to be visible
            wait.Until(d => d.FindElement(By.CssSelector(".accordion-collapse.show")));
        }

        [Then("they will see a button to export to jira")]
        public void ThenTheyWillSeeAButtonToExportToJira()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait for button to be fully interactable
            var exportButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.ClassName("exportJiraBtn")));

            // assert button exists
        }

        [Then("they will click the button")]
        public void AndTheyWillClickTheButton()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            // Wait for button to be fully interactable
            var exportButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.ClassName("exportJiraBtn")));

            exportButton.Click();
            // var for jira login
            // assert they are on login page
        }

        [Then("they will log into Jira")]
        public void AndTheyWillLogIntoJira()
        {
            // var for jira credientials
            // find login input
            // enter jira credentials 
            // click enter
            // wait until permissions page shows up
            // click permissions
            // wait until back on dashboard
            // assert back on dashboard
        }

        [Then("they will select the project to add to")]
        public void AndTheyWillSelectTheProjectToAddTo()
        {
            // find dropdown
            // click dropdown
            // select test project
            // click enter
        }

        [Then("they will see a loading spinner")]
        public void ThenTheyWillSeeALoadingSpinner()
        {
            //var for spinner
            // assert that loading spinner is visible
        }

        [Then("the Jira modal will close")]
        public void ThenTheJiraModalWillClose()
        {
            // find jira modal
            // assert that modal is not visible
        }
    }
}