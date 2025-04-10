using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Playwright;
using Reqnroll;

namespace Uxcheckmate_Main.StepDefinitions;

[Binding]
public class ScreenshotSteps
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IPage _page;

    [Given("David has submitted a valid URL for analysis")]
    public async Task GivenUserSubmitsValidUrl()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var context = await _browser.NewContextAsync();
        _page = await context.NewPageAsync();
        await _page.GotoAsync("http://localhost:5000");
        await _page.FillAsync("#urlInput", "https://example.com");
    }

    [Given("David clicks the \"Get Audit\" button")]
    public async Task GivenClickAuditButton()
    {
        await _page!.ClickAsync("#getAuditBtn");
    }

    [Given("a logo loader appears for two seconds")]
    public async Task GivenLoaderAppears()
    {
        var loader = await _page!.WaitForSelectorAsync("#loaderWrapper", new() { Timeout = 4000 });
        loader.Should().NotBeNull();
        await Task.Delay(2000); // Simulate loader duration
    }

    [Then("David should see a confirmation message and a placeholder displaying the website screenshot being captured")]
    public async Task ThenUserSeesMessageAndPlaceholder()
    {
        var scanningWrapper = await _page!.WaitForSelectorAsync("#scanningWrapper", new PageWaitForSelectorOptions { Timeout = 4000 });
        scanningWrapper.Should().NotBeNull("because the scanning wrapper should be visible after clicking the button");

        var spinner = await _page.QuerySelectorAsync(".spinner");
        spinner.Should().NotBeNull("because a spinner should be shown while the screenshot is being captured");

        var screenshotPlaceholder = await _page!.WaitForSelectorAsync("#screenshotPlaceholder", new() { Timeout = 60000, State = WaitForSelectorState.Visible });
        screenshotPlaceholder.Should().NotBeNull("because a screenshot placeholder should be shown");
    }

    [When("the analysis is complete")]
    public async Task WhenAnalysisIsComplete()
    {
        // var status = await _page!.WaitForSelectorAsync("#reportHeader", new() { Timeout = 10000 });
        // status.Should().NotBeNull("analysis should finish eventually");
    }

[Then("David should be redirected to the report page without a full page reload")]
public async Task ThenUserRedirectedToReport()
{
    Console.WriteLine("Current URL1: " + _page.Url);
    // Wait for the URL to change indicating the redirection
    await _page.WaitForURLAsync(url => url.Contains("/Home/Report"), new() { Timeout = 60000 });

    Console.WriteLine("Current URL2: " + _page.Url);

    var content = await _page.ContentAsync();
    Console.WriteLine("DEBUG PAGE HTML:\n" + _page.Url + content);

    // Verify that the URL is correct
    _page.Url.Should().Contain("/Home/Report");

    // Wait for a key element on the report page to ensure it's fully loaded
    var reportHeader = await _page.WaitForSelectorAsync("#reportHeader", new() { Timeout = 60000 });
    reportHeader.Should().NotBeNull("because the report header should be visible after redirection");
}

    [Then("David should see the screenshot in the report page")]
    public async Task ThenUserSeesScreenshot()
    {

        var screenshot = await _page!.WaitForSelectorAsync("#screenshotImage");
        screenshot.Should().NotBeNull("because the screenshot image should be present in the report page");

        var src = await screenshot!.GetAttributeAsync("src");
        src.Should().NotBeNullOrEmpty("screenshot should have an image source");
        src.Should().MatchRegex(@"data:image.*|screenshot.*", "it should show the dynamically captured screenshot");
    
        var content = await _page.ContentAsync();
        Console.WriteLine("DEBUG PAGE HTML:\n" + content);


    }

    [AfterScenario]
    public async Task Cleanup()
    {
        if (_page != null) await _page.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }
}
