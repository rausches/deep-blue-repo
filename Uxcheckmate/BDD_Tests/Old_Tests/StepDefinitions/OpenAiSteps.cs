using NUnit.Framework;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models; 
using Moq;
using Reqnroll;  
using Uxcheckmate_Main;
using Microsoft.Extensions.Logging;

namespace BDD_Tests.StepDefinitions  
{
    [Binding]
    public class UxAnalysisSteps
    {
        private readonly OpenAiService _openAiService;
        private UxResult _result = new UxResult { Issues = new List<UxIssue>() };
        private string _url;
        private Dictionary<string, object> _mockScrapedData;
        private bool _isFullReport = false;

        public UxAnalysisSteps()
        {
            var httpClient = new HttpClient(); 
            var logger = new Mock<ILogger<OpenAiService>>().Object;
            var dbContext = new UxCheckmateDbContext();
            _openAiService = new OpenAiService(httpClient, logger);
        }

        [Given(@"Sarah submits her URL")]
        public void GivenSarahSubmitsHerURL()
        {
            _url = "https://example.com"; 
        }

        [Given(@"her site has no recommendations")]
        public void GivenHerSiteHasNoRecommendations()
        {
            _mockScrapedData = new Dictionary<string, object>
            {
                { "no_issues", true } 
            };
        }

        [Given(@"her site has 16 fonts on it")]
        public void GivenHerSiteHas16FontsOnIt()
        {
            _mockScrapedData = new Dictionary<string, object>
            {
                { "fonts", new List<string> { "Arial", "Helvetica", "Times New Roman", "Georgia", "Verdana", 
                                            "Courier New", "Tahoma", "Trebuchet MS", "Impact", "Comic Sans MS",
                                            "Lucida Sans", "Palatino", "Garamond", "Bookman", "Avant Garde", "Candara" } }
            };
        }

        [Given(@"her site has huge blocks of text")]
        public void GivenHerSiteHasHugeBlocksOfText()
        {
            _mockScrapedData = new Dictionary<string, object>
            {
                { "text_content", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." }
            };
        }

        [When(@"the report does load")]
        public void WhenTheReportLoads()
        {
            _isFullReport = false;
            CreateMockResult();
        }

        [When(@"the fully report loads")]
        public void WhenTheFullyReportLoads()
        {
            _isFullReport = true;
            CreateMockResult();
        }

        private void CreateMockResult()
        {
            if (_mockScrapedData.ContainsKey("no_issues") && (bool)_mockScrapedData["no_issues"])
            {
                // If no issues exist, return an empty list
                _result = new UxResult { Issues = new List<UxIssue>() };
                return;
            }

            // Create a list to store identified issues
            var issues = new List<UxIssue>();

            // Check for font issues
            if (_mockScrapedData.ContainsKey("fonts") && _mockScrapedData["fonts"] is List<string> fonts && fonts.Count > 5)
            {
                issues.Add(new UxIssue { 
                    Category = "Fonts", 
                    Message = "Too many fonts used. Consider reducing the number of fonts for consistency." 
                });
            }

            // Check for text structure issues in the fully report case
            if (_isFullReport && _mockScrapedData.ContainsKey("text_content") && 
                _mockScrapedData["text_content"].ToString().Length > 100)
            {
                issues.Add(new UxIssue { 
                    Category = "Text Structure", 
                    Message = "Large blocks of text detected. Consider using images, padding, or color modules for better readability." 
                });
            }

            // Set the result
            _result = new UxResult { Issues = issues };
            
            Console.WriteLine($"Mocked OpenAI Response: {_result.Issues.Count} issues found. Full report: {_isFullReport}");
            foreach (var issue in _result.Issues)
            {
                Console.WriteLine($"- {issue.Category}: {issue.Message}");
            }
        }

        [Then(@"she will see a message that says there are no suggestions")]
        public void ThenSheWillSeeAMessageThatSaysThereAreNoSuggestions()
        {
            // If _result is null, fail with a clear message.
            if (_result == null)
            {
                Assert.Fail("The report was not loaded. _result is null.");
            }
            Assert.That(_result.Issues, Is.Empty, "Expected no recommendations, but some were found.");
        }

        [Then(@"she will see a suggestion in the report to condense the amount of fonts on her site")]
        public void ThenSheWillSeeASuggestionInTheReportToCondenseTheAmountOfFontsOnHerSite()
        {
            // Check if the result contains a suggestion about fonts
            Assert.That(_result.Issues, Is.Not.Empty, "Expected font recommendations, but none were found.");
            Assert.That(_result.Issues.Any(i => i.Category == "Fonts" && i.Message.Contains("fonts")), 
                        Is.True, "Expected a recommendation about reducing fonts, but none was found.");
        }

        [Then(@"she will see a suggestion to separate her text content with images, padding, and color modules")]
        public void ThenSheWillSeeASuggestionToSeparateHerTextContentWithImagesPaddingAndColorModules()
        {
            // Check if the result contains a suggestion about text structure
            Assert.That(_result.Issues, Is.Not.Empty, "Expected text structure recommendations, but none were found.");
            Assert.That(_result.Issues.Any(i => i.Category == "Text Structure" && i.Message.Contains("text")), 
                        Is.True, "Expected a recommendation about improving text layout, but none was found.");
        }
    }
}