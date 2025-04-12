using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Uxcheckmate_Main.Services;

namespace Service_Tests
{
    [TestFixture]
    public class AudioService_Tests
    {
        private AudioService _audioService;
        private Mock<ILogger<AudioService>> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<AudioService>>();
            _audioService = new AudioService(_loggerMock.Object);
        }

        [Test]
        public async Task RunAudioAnalysisAsync_ReturnsFinding_When_AudioTagFound()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", "<audio autoplay><source src='test.mp3'></audio>" },
                { "externalJsContents", new List<string>() }
            };

            // Act
            var result = await _audioService.RunAudioAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("audio-related"));
        }

        [Test]
        public async Task RunAudioAnalysisAsync_ReturnsFinding_When_JavaScriptAudioFound()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", "<div>No audio in HTML</div>" },
                { "externalJsContents", new List<string> { "var sound = new Audio('sound.mp3'); sound.play();" } }
            };

            // Act
            var result = await _audioService.RunAudioAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Not.Empty);
            Assert.That(result, Does.Contain("audio-related"));
        }

        [Test]
        public async Task RunAudioAnalysisAsync_ReturnsEmpty_When_NoAudioContentExists()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "htmlContent", "<div>Just text</div>" },
                { "externalJsContents", new List<string> { "console.log('hello');" } }
            };

            // Act
            var result = await _audioService.RunAudioAnalysisAsync("https://example.com", scrapedData);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
