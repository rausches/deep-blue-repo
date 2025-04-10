using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Reqnroll;
using System;

namespace Uxcheckmate.BDD_Tests.StepDefinitions
{
    [Binding]
    public class Hooks
    {
        private IWebDriver _driver;
        private readonly ScenarioContext _scenarioContext;

        public Hooks(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            _driver = new ChromeDriver();
            _scenarioContext.ScenarioContainer.RegisterInstanceAs<IWebDriver>(_driver);
        }

        [AfterScenario]
        public void AfterScenario()
        {
            _driver?.Quit();
        }
    }
}
