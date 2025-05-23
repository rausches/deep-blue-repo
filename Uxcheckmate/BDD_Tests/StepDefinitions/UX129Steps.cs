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
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            // Wait for button to be fully interactable
            var exportButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.ClassName("exportJiraBtn")));

            exportButton.Click();
        }

        [Then("they will log into Jira")]
        public void AndTheyWillLogIntoJira()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));
            // Jira login
            var username = "t8604369@gmail.com";
            var password = "6N@*wZJP$m3FR2a";

            // Enter Username
            var jiraLogin = wait.Until(d => d.FindElement(By.Id("username")));
            jiraLogin.Clear();
            jiraLogin.SendKeys(username);
            var submitBtn = wait.Until(d => d.FindElement(By.Id("login-submit")));
            submitBtn.Click();
            
            // Enter Password
            var passwordInput =  wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.Id("password")));
            passwordInput.Clear();
            passwordInput.SendKeys(password);
            submitBtn.Click();

            var acceptBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions
                .ElementToBeClickable(By.ClassName("css-1dw4iom")));
            acceptBtn.Click();
        }

        [Then("they will select the project to add to")]
        public void AndTheyWillSelectTheProjectToAddTo()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            // Wait until we get back to dashboard
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("projectSelectModal")));

            // Open dropdown options and select project
            var projectDropdown = new SelectElement(_driver.FindElement(By.Id("jiraProjectDropdown")));
            projectDropdown.SelectByText("TES - TestTest");

            // Click export
            var confirmProjectBtn = wait.Until(d => d.FindElement(By.Id("confirmProjectButton")));
            confirmProjectBtn.Click();
        }

        [Then("they will see a loading spinner")]
        public void ThenTheyWillSeeALoadingSpinner()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            // Spinner is visible
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("spinner-border")));
        }

        [Then("the Jira modal will close")]
        public void ThenTheJiraModalWillClose()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(180));

            bool isInvisible = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(By.Id("projectSelectModal")));

            // Assert that the modal is gone
            Assert.That(isInvisible, Is.True);
        }
    }
}
