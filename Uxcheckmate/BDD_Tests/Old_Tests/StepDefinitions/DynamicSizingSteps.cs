/*using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Uxcheckmate_Main.Services;

[Binding]
public class DynamicSizingSteps
{
    private readonly IDynamicSizingService _dynamicSizingService;
    private bool _hasDynamicSizing;
    private string? _reportMessage;

    public DynamicSizingSteps()
    {
        // Create a logger
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(); 
        });
        var logger = loggerFactory.CreateLogger<DynamicSizingService>();

        // Instantiate the DynamicSizingService
        _dynamicSizingService = new DynamicSizingService();
    }

    // 
    [Given(@"I am (.*)")]
    public void GivenIAmUser(string user)
    {
        // No action needed
    }

    [Given(@"my site has no dynamic sizing elements")]
    public void GivenMySiteHasNoDynamicSizingElements()
    {
        _hasDynamicSizing = false;
    }

    [Given(@"my site has proper dynamic sizing elements")]
    public void GivenMySiteHasProperDynamicSizingElements()
    {
        _hasDynamicSizing = true;
    }

    [When(@"I enter the url of my site")]
    public async Task WhenIEnterTheUrlOfMySite()
    {
        string fakeHtmlContent = _hasDynamicSizing
            ? "<meta name='viewport' content='width=device-width, initial-scale=1'><style>@media (max-width: 600px) { body { font-size: 14px; } }</style>"
            : "<html><head></head><body><h1>Static Content</h1></body></html>";

        var scrapedData = new Dictionary<string, object> { { "htmlContent", fakeHtmlContent } };

        // Simulate checking dynamic sizing
        var hasDynamicSizing = _dynamicSizingService.HasDynamicSizing(scrapedData["htmlContent"].ToString());

        if (!hasDynamicSizing)
        {
            _reportMessage = "The report should let me know I need to add dynamic sizing and why.";
        }
        else
        {
            _reportMessage = string.Empty;
        }
    }

    [When(@"the report loads")]
    public void WhenTheReportLoads()
    {
        _reportMessage.Should().NotBeNull();
    }

    [Then(@"the report should let me know I need to add dynamic sizing and why")]
    public void ThenTheReportShouldNotifyAboutMissingDynamicSizing()
    {
        _reportMessage.Should().Contain("dynamic sizing");
    }

    [Then(@"the report should not tell me that it may look bad on a phone")]
    public void ThenTheReportShouldNotMentionDynamicSizingIssues()
    {
        _reportMessage.Should().BeEmpty();
    }
}
*/