using Reqnroll;
using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Controllers;
using Uxcheckmate_Main.Services;

[Binding]
public class Pa11yAccessiblitySteps
{
    private readonly AccessibilityController _controller;
    private IActionResult _response;
    private string _submittedUrl;
    public Pa11yAccessiblitySteps()
    {
        // Setting Up Mock In order to Build Html
        var handlerMock = new Mock<HttpMessageHandler>();
        // Missing Alt and Perfect sit mock Build
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
            {
                if (request.RequestUri.ToString().Contains("missingAlt")){
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("<html><body><img src='image.jpg'></body></html>")
                    };
                }
                return new HttpResponseMessage{
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("<html><body><img src='image.jpg' alt='A test image'></body></html>")
                };
            });
        var fakeHttpClient = new HttpClient(handlerMock.Object);
        var scraperService = new WebScraperService(fakeHttpClient);
        var logger = NullLogger<AccessibilityController>.Instance;
        _controller = new AccessibilityController(scraperService, logger);
    }
    [Given(@"Sarah submits her URL ""(.*)""")]
    public void GivenSarahSubmitsHerURL(string url)
    {
        _submittedUrl = url;
    }
    [When(@"the accessibility report loads")]
    public async Task WhenTheAccessibilityReportLoads()
    {
        var formContent = new MultipartFormDataContent
        {
            { new StringContent(_submittedUrl), "targetUrl" }
        };

        _response = await _controller.CheckAccessibility(_submittedUrl);
    }
    [Then(@"she sees an error for an image without an alt tag")]
    public void ThenSheSeesAnErrorForAnImageWithoutAnAltTag()
    {
        var result = _response as ObjectResult;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        result.Value.ToString().Should().Contain("Image missing alt tag");
    }
    [Then(@"she sees a message that there were no errors found")]
    public void ThenSheSeesAMessageThatThereWereNoErrorsFound()
    {
        var result = _response as ObjectResult;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);
        result.Value.ToString().Should().Contain("No accessibility errors found");
    }
}