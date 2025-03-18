using NUnit.Framework;
using System.Net.Http;
using System.Threading.Tasks;
using Uxcheckmate_Main.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Nunit_Tests
{
    [TestFixture]
    public class WebScraperServiceLiveTests
    {
        private HttpClient _httpClient;
        private WebScraperService _scraperService;
        private Mock<ILogger<WebScraperService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _httpClient = new HttpClient();
            _loggerMock = new Mock<ILogger<WebScraperService>>();
            _scraperService = new WebScraperService(_httpClient, _loggerMock.Object);
        }

        [TearDown]
        public void Cleanup()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task ScrapeAsync_ShouldReturnValidData_MarinaOfficial()
        {
            // Arrange
            var url = "https://marinaofficial.co.uk/";

            // Act
            var result = await _scraperService.ScrapeAsync(url);

            // Assert
            Assert.That(result, Is.Not.Null, "ScrapeAsync returned null.");
            Assert.That((int)result["headings"], Is.GreaterThanOrEqualTo(0), "No headings found.");
            Assert.That((int)result["images"], Is.GreaterThanOrEqualTo(0), "No images found.");
            Assert.That(result.ContainsKey("hasFavicon"), "Favicon data missing.");
            Assert.That(result["links"], Is.TypeOf<List<string>>(), "Links data type mismatch.");

            TestContext.WriteLine($"Headings: {result["headings"]}, Images: {result["images"]}");
        }

        [Test]
        public async Task ScrapeAsync_ShouldReturnValidData_WouEdu()
        {
            // Arrange
            var url = "https://wou.edu/";

            // Act
            var result = await _scraperService.ScrapeAsync(url);

            // Assert
            Assert.That(result, Is.Not.Null, "ScrapeAsync returned null.");
            Assert.That((int)result["headings"], Is.GreaterThanOrEqualTo(0), "No headings found.");
            Assert.That((int)result["images"], Is.GreaterThanOrEqualTo(0), "No images found.");
            Assert.That(result.ContainsKey("hasFavicon"), "Favicon data missing.");
            Assert.That(result["links"], Is.TypeOf<List<string>>(), "Links data type mismatch.");

            TestContext.WriteLine($"Headings: {result["headings"]}, Images: {result["images"]}");
        }
    }
}
