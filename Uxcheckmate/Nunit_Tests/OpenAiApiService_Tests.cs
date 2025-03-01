// using NUnit.Framework;
// using System.Collections.Generic;
// using System.Text;
// using Uxcheckmate_Main.Services;
// using Uxcheckmate_Main.Models;
// using Microsoft.Extensions.Logging.Abstractions;
// using Microsoft.EntityFrameworkCore;

// namespace Uxcheckmate_Tests.Services
// {
//     [TestFixture]
//     public class OpenAiServiceUnitTests
//     {
//         private OpenAiService _openAiService;

//         [SetUp]
//         public void Setup()
//         {
//             var httpClient = new HttpClient();

//             // Use built-in NullLogger
//             var logger = NullLogger<OpenAiService>.Instance;

//             _openAiService = new OpenAiService(httpClient, logger);
//         }

//         /* Test if FormatScrapedData function returns a formatted string */
//         [Test]
//         public void FormatScrapedData_ProducesExpectedOutput()
//         {
//             // Arrange: Create sample scraped data dictionary.
//             var scrapedData = new Dictionary<string, object>
//             {
//                 { "headings", 5 },
//                 { "images", 10 },
//                 { "links", 8 },
//                 { "fonts", new List<string> { "Arial", "Roboto", "Verdana" } },
//                 { "text_content", "This is sample text for testing purposes." }
//             };

//             // Build the expected formatted output using Environment.NewLine.
//             string expected = string.Join(Environment.NewLine, new[]
//             {
//                 "Headings Count: 5",
//                 "Images Count: 10",
//                 "Links Count: 8",
//                 "Fonts Used: Arial, Roboto, Verdana",
//                 "This is sample text for testing purposes.",
//                 "" // Represents the trailing newline.
//             });

//             // Act: Use reflection to invoke the private FormatScrapedData method.
//             var method = _openAiService.GetType()
//                 .GetMethod("FormatScrapedData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//             var result = method.Invoke(_openAiService, new object[] { scrapedData }) as string;

//             // Assert: Ensure the result is not null or empty and matches the expected output.
//             Assert.That(result, Is.Not.Null.And.Not.Empty, "FormatScrapedData returned null or empty string.");
//             Assert.That(result.Trim(), Is.EqualTo(expected.Trim()), "The formatted output does not match the expected value.");
//         }

//     }
// }
