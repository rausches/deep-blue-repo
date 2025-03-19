using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Uxcheckmate_Main.Services;

[Binding]
public class BrokenLinksSteps
{
    private readonly IBrokenLinksService _brokenLinksService;
    private string _siteUrl;
    private List<string> _reportResults;

    public BrokenLinksSteps()
    {
        // Create the HttpClient instance
        var httpClient = new HttpClient();

        // Create a logger using LoggerFactory
        var loggerFactory = LoggerFactory.Create(builder => 
        {
            builder.AddConsole(); 
        });
        var logger = loggerFactory.CreateLogger<BrokenLinksService>();

        // Instantiate the service with the required parameters
        _brokenLinksService = new BrokenLinksService(httpClient, logger);
        _reportResults = new List<string>();
    }

    [Given(@"(.*) has a site with a broken link")]
    public void GivenUserHasASiteWithABrokenLink(string user)
    {
        _siteUrl = user == "David" ? "https://wou.edu/feet" : string.Empty;
    }

    [Given(@"(.*) has a site with no broken links")]
    public void GivenUserHasASiteWithNoBrokenLinks(string user)
    {
        _siteUrl = user == "Sarah" ? "https://wou.edu/marcom" : string.Empty;
    }

    [Given(@"he enters his site url into the url submission box")]
    [Given(@"she enters her site url into the url submission box")]
    public async Task GivenUserEntersSiteUrlIntoTheUrlSubmissionBox()
    {
        _reportResults = await _brokenLinksService.CheckBrokenLinksAsync(new List<string> { _siteUrl });
    }

    [Given(@"the report loads")]
    public void GivenTheReportLoads()
    {
        _reportResults.Should().NotBeNull();
    }

    [Then(@"he should see the URL listed and be told the status code error")]
    public void ThenUserShouldSeeUrlListedWithStatusCodeError()
    {
        _reportResults.Should().Contain(x => x.Contains(_siteUrl) || x.Contains("No such host is known"));
    }

    [Then(@"she should not see anything regarding broken links in the report")]
    public void ThenUserShouldNotSeeAnythingRegardingBrokenLinks()
    {
        _reportResults.Should().BeEmpty();
    }
}
