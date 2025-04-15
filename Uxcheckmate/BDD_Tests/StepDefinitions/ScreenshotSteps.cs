using Microsoft.AspNetCore.Mvc;
using Moq;
using Reqnroll;
using System.Security.Claims;
using Uxcheckmate_Main.Controllers;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Uxcheckmate_Main.StepDefinitions;

[Binding]
public class ScreenshotSteps
{
    private readonly IWebDriver driver;
    private HomeController _controller;
    private IActionResult _result;
    public ScreenshotSteps(IWebDriver webDriver)
    {
        driver = webDriver;
    }

    // // Mock additions
    // [Given("the user navigates to the site")]
    // public void GivenTheUserNavigatesToTheSite()
    // {
    //     driver.Navigate().GoToUrl("http://localhost:5000/");
    // }
 
    // [When(@"the user enters a URL to analyze(?: with ""(.*)"")?")]
    // public void WhenTheUserEntersAUrlToAnalyze(string url = null)
    // {
    //     url ??= "https://example.com";
    //     driver.FindElement(By.Id("urlInput")).SendKeys(url);
    // }

    // [When("the user starts the analysis")]
    // public void WhenTheUserStartsTheAnalysis()
    // {
    //     var submitButton = driver.FindElement(By.Id("analyzeBtn"));
    //     submitButton.Click();
    // } 

    [Then("Then the user will see a loading overlay")]
    public void ThenUserSeesLoadingOverlay()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        wait.Until(driver => driver.FindElement(By.Id("scanningWrapper")));
    }

    [Then("Then the user should see the result view with the website screenshot")]
    public void ThenUserSeesResultViewWithScreenshot()
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
        wait.Until(driver => driver.FindElement(By.Id("screenshot")));
        var screenshotElement = driver.FindElement(By.Id("screenshot"));
        Assert.That(screenshotElement.Displayed, Is.True, "Screenshot element is not displayed.");
    }


}