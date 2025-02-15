using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Tests.Services
{
    [TestFixture]
    public class OpenAiServiceUnitTests
    {
        private OpenAiService _openAiService;

        [SetUp]
        public void Setup()
        {
            _openAiService = new OpenAiService(null, null); 
        }


        /* Test if FormatScrapedData function returns a formatted string */
        [Test]
        public void FormatScrapedData_ReturnFormattedString()
        {
            // Arrange
            var scrapedData = new Dictionary<string, object>
            {
                { "headings", 5 },
                { "images", 10 },
                { "links", 8 },
                { "fonts", new List<string> { "Arial", "Roboto", "Verdana" } },
                { "text_content", "This is sample text for testing purposes." }
            };

            // Act
            var result = _openAiService.GetType()
                .GetMethod("FormatScrapedData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_openAiService, new object[] { scrapedData }) as string;

            // Assert
            Assert.That(result, Is.Not.Null.And.Not.Empty, "FormatScrapedData returned null or empty string.");

            Assert.That(result, Does.Contain("Headings Count: 5"), "Missing 'Headings Count: 5' in formatted data.");
            Assert.That(result, Does.Contain("Images Count: 10"), "Missing 'Images Count: 10' in formatted data.");
            Assert.That(result, Does.Contain("Links Count: 8"), "Missing 'Links Count: 8' in formatted data.");
            Assert.That(result, Does.Contain("Fonts Used: Arial, Roboto, Verdana"), "Missing font list in formatted data.");
            Assert.That(result, Does.Contain("This is sample text for testing purposes."), "Missing text content in formatted data.");
        }

        /* Test if ExtractSections function returns a dictionary */
        [Test]
        public void ExtractSections_ReturnsDictionary()
        {
        }



        /* Tests if  */
        [Test]
        public void ConvertToUxResult_ReturnsUxResult()
        {
        }

    }
}
