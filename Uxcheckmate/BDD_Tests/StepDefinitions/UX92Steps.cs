using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using System;

namespace BDD_Tests.StepDefinitions
{
    [Binding]
    public class UserFeedbackSteps
    {
        private readonly IWebDriver _driver;

        public UserFeedbackSteps(IWebDriver driver)
        {
            _driver = driver;
        }
        [When(@"they go to feedback as ""(.*)""")]
        public void WhenTheyGoToFeedbackAs(string role)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            string feedbackUrl = role == "Admin" ? "/Admin/AdminFeedback" : "/Home/Feedback";
            wait.Until(driver =>
            {
                try{
                    var feedbackLink = driver.FindElement(By.XPath($"//a[@href='{feedbackUrl}']"));
                    feedbackLink.Click();
                    return true;
                }catch{
                    return false;
                }
            });
            if (role == "Admin"){
                wait.Until(driver => driver.FindElement(By.XPath("//h1[text()='User Feedback']")));
            }else{
                wait.Until(driver => driver.FindElement(By.Id("Message")));
            }
        }
        [When(@"enter feedback ""(.*)""")]
        public void WhenEnterFeedback(string feedbackText)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var input = wait.Until(d => d.FindElement(By.Id("Message")));
            input.Clear();
            input.SendKeys(feedbackText);
            var submit = wait.Until(d => d.FindElement(By.Id("submitFeedback")));
            submit.Click();
        }
        [Then("they should see a success message")]
        public void ThenTheyShouldSeeASuccessMessage()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var alert = wait.Until(d => d.FindElement(By.ClassName("alert-success")));
            Assert.That(alert.Text.ToLower(), Does.Contain("submitted"));
        }
        [Then("they should see user feedback list")]
        public void ThenTheyShouldSeeTheFeedbackList()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var header = wait.Until(d => d.FindElement(By.XPath("//h1[text()='User Feedback']")));
            Assert.That(header.Displayed, Is.True, "Feedback header was not found.");
        }
        [Then("they should see a validation error")]
        public void ThenTheyShouldSeeAValidationError()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var error = wait.Until(d => d.FindElement(By.ClassName("validation-summary-errors")));
            Assert.That(error.Text.ToLower(), Does.Contain("feedback"), "No validation message found.");
        }
    }
}
