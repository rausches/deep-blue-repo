using NUnit.Framework;
using Uxcheckmate_Main.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Service_Tests
{
    [TestFixture]
    public class ColorSchemeServiceTests
    {
        private ColorSchemeService _colorService;
        private Mock<WebScraperService> _webScraperServiceMock;
        private Mock<ILogger<ColorSchemeService>> _loggerMock;
        private Mock<IScreenshotService> _screenshotServiceMock;
        [SetUp]
        public void Setup()
        {
            _webScraperServiceMock = new Mock<WebScraperService>(new HttpClient(), Mock.Of<ILogger<WebScraperService>>());
            _loggerMock = new Mock<ILogger<ColorSchemeService>>();
            _screenshotServiceMock = new Mock<IScreenshotService>();
            _colorService = new ColorSchemeService(_webScraperServiceMock.Object, _loggerMock.Object, _screenshotServiceMock.Object);
        }
        [Test]
        public void MaxDifference_AreColorsSimilar_True()
        {
            // Setting Max 
            var color1 = (255, 100, 50);
            var color2 = (205, 100, 50);
            bool result = ColorSchemeService.AreColorsSimilar(color1, color2);
            Assert.That(result, Is.True, "Colors should be considered similar.");
        }

        [Test]
        public void DiffrentColors_AreColorsSimilar_False()
        {
            var color1 = (255, 255, 255);
            var color2 = (0, 0, 0);
            bool result = ColorSchemeService.AreColorsSimilar(color1, color2);
            Assert.That(result, Is.False, "Colors should be considered different.");
        }

        [Test]
        public void HexToRgb_ReturnsCorrectRgb()
        {
            var result = ColorSchemeService.HexToRgb("#FF5733");
            Assert.That(result, Is.EqualTo((255, 87, 51)), "Hex to RGB conversion failed.");
        }

        [Test]
        public void HexToRgb_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => ColorSchemeService.HexToRgb("#ZZZZZZ"));
        }
        // Testing Colors From Classes Being Stored 
        [Test]
        public void ClassColorsGetExtracted()
        {
            // Creating Blue Text Class
            string htmlContent = @"
                <style>
                    .blue-text { color: #0000FF; }
                </style>
                <h1 class='blue-text'>Hello</h1>
                <h2>World</h2>";
            // No Css just HTML so making blank
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            // Should see that there is blue-text
            var classColors = (Dictionary<string, string>)result["class_colors"];
            Assert.That(classColors.ContainsKey("blue-text"), "Class color should be detected.");
            Assert.That(classColors["blue-text"], Is.EqualTo("#0000FF"), "Class blue-text should be assigned #0000FF.");
        }
        // Inline-Style Check
        [Test]
        public void ExtractingInlineStyles()
        {
            // Creating p with inline
            string htmlContent = @"<p style='color: #FF5733;'>Sample Text</p>";
            // No Css
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagColors = (Dictionary<string, string>)result["tag_colors"];
            // Should find color associated with that p
            Assert.That(tagColors.ContainsKey("p"), "Tag color should be detected.");
            Assert.That(tagColors["p"], Is.EqualTo("#FF5733"), "Paragraph should be assigned #FF5733.");
        }
        // Testing that characters in class are counted towards class and not tag
        [Test]
        public void AssigningClassCharacterCount()
        {
            string htmlContent = @"
                <style>
                    .red-text { color: #FF0000; }
                </style>
                <h1 class='red-text'>Important</h1>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            // Couting chracters in class versus tag
            var classCharacterCount = (Dictionary<string, int>)result["character_count_per_class"];
            var tagCharacterCount = (Dictionary<string, int>)result["character_count_per_tag"];
            // Making sure there are 9 characters in class and none in tag
            Assert.That(classCharacterCount, Contains.Key("red-text"), "Character count should be assigned to class.");
            Assert.That(classCharacterCount["red-text"], Is.EqualTo(9), "Class 'red-text' should have 9 characters.");
            Assert.That(tagCharacterCount, Does.Not.ContainKey("h1"), "h1 should not count characters because it's assigned to class.");
        }
        // If no color associated with specific tag use then it should default to black
        [Test]
        public void TagsShouldDefaultToBlack()
        {
            // Basic one line html test
            string htmlContent = @"<h2>Default Color</h2>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagColors = (Dictionary<string, string>)result["tag_colors"];
            // Should have the tag color of h2 associated with h2 to be black
            Assert.That(tagColors.ContainsKey("h2"), "Tag should be detected.");
            Assert.That(tagColors["h2"], Is.EqualTo("#000000"), "Should default to black.");
        }
        // No class means character count is added to tag
        [Test]
        public void CharactersWithoutColorClassesAreInTag()
        {
            // html without classes
            string htmlContent = @"<p>Text in a paragraph.</p>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagCharacterCount = (Dictionary<string, int>)result["character_count_per_tag"];
            // p should show up and have 21 characters
            Assert.That(tagCharacterCount, Contains.Key("p"), "Paragraph should be counted.");
            Assert.That(tagCharacterCount["p"], Is.EqualTo(20), "Paragraph should have 21 characters.");
        }
        [Test]
        public void DefaultSizingForTags()
        {
            // Html without sizing
            string htmlContent = @"
                <h1>Main Header</h1>
                <p>Paragraph text.</p>";       
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagFontSizes = (Dictionary<string, int>)result["tag_font_sizes"];
            // h1 needs to be 32 px and p needs to be 16px
            Assert.That(tagFontSizes, Contains.Key("h1"), "H1 tag should be detected.");
            Assert.That(tagFontSizes["h1"], Is.EqualTo(32), "Default font size for <h1> should be 32px.");
            Assert.That(tagFontSizes, Contains.Key("p"), "P tag should be detected.");
            Assert.That(tagFontSizes["p"], Is.EqualTo(16), "Default font size for <p> should be 16px.");
        }
        [Test]
        public void ShouldUseClassFontSize()
        {
            // Class font size
            string htmlContent = @"
                <style>
                    .large-text { font-size: 24px; }
                </style>
                <p class='large-text'>Big text</p>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var classFontSizes = (Dictionary<string, int>)result["class_font_sizes"];
            Assert.That(classFontSizes, Contains.Key("large-text"), "Class should have a font size.");
            Assert.That(classFontSizes["large-text"], Is.EqualTo(24), "Class 'large-text' should have a font size of 24px.");
        }
        [Test]
        public void TagInlineFontSize()
        {
            // Inline font size
            string htmlContent = @"<h2 style='font-size: 30px;'>Big Heading</h2>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagFontSizes = (Dictionary<string, int>)result["tag_font_sizes"];
            Assert.That(tagFontSizes, Contains.Key("h2"), "Inline font size should be detected.");
            // Should contain inline size
            Assert.That(tagFontSizes["h2"], Is.EqualTo(30), "Inline font size for <h2> should override default.");
        }
        [Test]
        public void InlineOverClassFontsize()
        {
            // Class with inline and overidden by inline
            string htmlContent = @"
                <style>
                    .medium-text { font-size: 22px; }
                </style>
                <h3 class='medium-text' style='font-size: 28px;'>Larger Text</h3>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagFontSizes = (Dictionary<string, int>)result["tag_font_sizes"];
            var classFontSizes = (Dictionary<string, int>)result["class_font_sizes"];
            // Should still contain class font
            Assert.That(classFontSizes.ContainsKey("medium-text"), "Class font size should be detected.");
            Assert.That(classFontSizes["medium-text"], Is.EqualTo(22), "Class 'medium-text' should have 22px.");
            // However it should add to inline
            Assert.That(tagFontSizes.ContainsKey("h3"), "Tag font size should be detected.");
            Assert.That(tagFontSizes["h3"], Is.EqualTo(28), "Inline font size (28px) should override class size.");
        }
        [Test]
        public void NoFontSizeChangeInClass()
        {
            string htmlContent = @"
                <style>
                    .no-size { color: red; }
                </style>
                <h4 class='no-size'>Small Header</h4>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var tagFontSizes = (Dictionary<string, int>)result["tag_font_sizes"];
            // Should set for default
            Assert.That(tagFontSizes.ContainsKey("h4"), "Default font size should be assigned.");
            Assert.That(tagFontSizes["h4"], Is.EqualTo(16), "Default size for <h4> should be 16px if not explicitly set.");
        }
        [Test]
        public void CountCharactersWithFontSizes()
        {
            string htmlContent = @"
                <style>
                    .big { font-size: 30px; }
                </style>
                <h1 class='big'>Huge</h1>
                <p style='font-size: 12px;'>Small</p>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            // Setup for counting characters
            var tagFontSizes = (Dictionary<string, int>)result["tag_font_sizes"];
            var classFontSizes = (Dictionary<string, int>)result["class_font_sizes"];
            var tagCharacterCount = (Dictionary<string, int>)result["character_count_per_tag"];
            var classCharacterCount = (Dictionary<string, int>)result["character_count_per_class"];
            // Checking each match with size and character amount
            Assert.That(classFontSizes, Contains.Key("big"), "Class 'big' should have font size.");
            Assert.That(classFontSizes["big"], Is.EqualTo(30), "Class 'big' should have font size 30px.");
            Assert.That(tagFontSizes, Contains.Key("p"), "Paragraph should have an inline font size.");
            Assert.That(tagFontSizes["p"], Is.EqualTo(12), "Paragraph should have an inline font size of 12px.");
            Assert.That(classCharacterCount, Contains.Key("big"), "Class 'big' should have character count.");
            Assert.That(classCharacterCount["big"], Is.EqualTo(4), "Class 'big' should count 4 characters from <h1>.");
            Assert.That(tagCharacterCount, Contains.Key("p"), "Paragraph should count characters.");
            Assert.That(tagCharacterCount["p"], Is.EqualTo(5), "Paragraph should count 5 characters.");
        }
        [Test]
        public void BackgroundColorShouldBeExtracted()
        {
            string htmlContent = @"
                <style>
                    body { background-color: #F0F0F0; }
                </style>
                <h1>Title</h1>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var backgroundColor = (string)result["background_color"];
            Assert.That(backgroundColor, Is.EqualTo("#F0F0F0"), "Background color should be extracted.");
        }
        [Test]
        public void BackgroundDefaultsToWhiteIfNoneSpecified()
        {
            string htmlContent = @"
                <html>
                <head></head>
                <body>
                    <h1>Title</h1>
                </body>
                </html>";
            List<string> externalCss = new List<string>();
            var result = _colorService.ExtractHtmlElements(htmlContent, externalCss);
            var backgroundColor = (string)result["background_color"];
            Assert.That(backgroundColor, Is.EqualTo("#FFFFFF"), "Background color should default to white if unspecified.");
        }
        [Test]
        public void CalculatePixelUsage()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int> { { "p", 50 }, { "h1", 20 } } },
                { "character_count_per_class", new Dictionary<string, int> { { "classClass", 30 } } },
                { "tag_font_sizes", new Dictionary<string, int> { { "p", 16 }, { "h1", 32 } } },
                { "class_font_sizes", new Dictionary<string, int> { { "classClass", 24 } } },
                { "tag_colors", new Dictionary<string, string> { { "p", "#000000" }, { "h1", "#FF0000" } } },
                { "class_colors", new Dictionary<string, string> { { "classClass", "#00FF00" } } }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            Assert.That(result["#000000"], Is.EqualTo(50 * (16 * 0.5) * 16), "Pixel area for <p> should be correctly calculated.");
            Assert.That(result["#FF0000"], Is.EqualTo(20 * (32 * 0.5) * 32), "Pixel area for <h1> should be correctly calculated.");
            Assert.That(result["#00FF00"], Is.EqualTo(30 * (24 * 0.5) * 24), "Pixel area for 'classClass' should be correctly calculated.");
        }
        [Test]
        public void ColorPixelDefaultFontSize()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int> { { "p", 40 } } },
                { "character_count_per_class", new Dictionary<string, int>() },
                { "tag_font_sizes", new Dictionary<string, int>() },
                { "class_font_sizes", new Dictionary<string, int>() },
                { "tag_colors", new Dictionary<string, string> { { "p", "#000000" } } },
                { "class_colors", new Dictionary<string, string>() }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            int expectedPixels = (int)(40 * (16 * 0.5) * 16);
            Assert.That(result["#000000"], Is.EqualTo(expectedPixels), "Should use default font size of 16px for <p> when missing.");
        }
        [Test]
        public void ColorPixelLargeValues()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int> { { "h1", 10000 } } },
                { "character_count_per_class", new Dictionary<string, int>() },
                { "tag_font_sizes", new Dictionary<string, int> { { "h1", 32 } } },
                { "class_font_sizes", new Dictionary<string, int>() },
                { "tag_colors", new Dictionary<string, string> { { "h1", "#FF0000" } } },
                { "class_colors", new Dictionary<string, string>() }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            Assert.That(result["#FF0000"], Is.EqualTo(10000 * (32 * 0.5) * 32), "Should correctly compute large pixel values.");
        }
        [Test]
        public void TextSubtractsBackground()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int> { { "p", 40 } } },
                { "character_count_per_class", new Dictionary<string, int>() },
                { "tag_font_sizes", new Dictionary<string, int> { { "p", 16 } } },
                { "class_font_sizes", new Dictionary<string, int>() },
                { "tag_colors", new Dictionary<string, string> { { "p", "#000000" } } },
                { "class_colors", new Dictionary<string, string>() },
                { "background_color", "#FFFFFF" }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            int expectedTextPixels = (int)(40 * (16 * 0.5) * 16);
            int expectedBackgroundPixels = 2_073_600 - expectedTextPixels;
            Assert.That(result.ContainsKey("#000000"), "Text color should be included.");
            Assert.That(result["#000000"], Is.EqualTo(expectedTextPixels), "Text pixels should be correctly calculated.");
            Assert.That(result.ContainsKey("#FFFFFF"), "Background color should be included.");
            Assert.That(result["#FFFFFF"], Is.EqualTo(expectedBackgroundPixels), "Background pixels should be correctly adjusted.");
        }
        [Test]
        public void EmptyColorPixelInput()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int>() },
                { "character_count_per_class", new Dictionary<string, int>() },
                { "tag_font_sizes", new Dictionary<string, int>() },
                { "class_font_sizes", new Dictionary<string, int>() },
                { "tag_colors", new Dictionary<string, string>() },
                { "class_colors", new Dictionary<string, string>() },
                { "background_color", "#FFFFFF" }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            Assert.That(result.Count, Is.EqualTo(1), "Only background pixels should be counted.");
            Assert.That(result.ContainsKey("#FFFFFF"), "Background color should be stored.");
            Assert.That(result["#FFFFFF"], Is.EqualTo(2073600), "All pixels should be background color.");
        }
        [Test]
        public void EmptyColorPixelInputWithNoBackground()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "character_count_per_tag", new Dictionary<string, int>() },
                { "character_count_per_class", new Dictionary<string, int>() },
                { "tag_font_sizes", new Dictionary<string, int>() },
                { "class_font_sizes", new Dictionary<string, int>() },
                { "tag_colors", new Dictionary<string, string>() },
                { "class_colors", new Dictionary<string, string>() }
            };
            var result = _colorService.EstimateColorPixelUsage(extractedData);
            Assert.That(result.ContainsKey("#FFFFFF"), "Background color should be assigned when no text elements exist.");
            Assert.That(result["#FFFFFF"], Is.EqualTo(2_073_600), "All pixels should be assigned to the background.");
        }
        [Test]
        public void EmptyColorProportions()
        {
            var result = _colorService.CalculateColorProportions(new Dictionary<string, int>());
            Assert.That(result, Is.Empty, "Should return an empty dictionary when there are no colors.");
        }
        [Test]
        public void CalculateColorProportionsCorrect()
        {
            var colorPixelUsage = new Dictionary<string, int>
            {
                { "#FF0000", 500 },
                { "#00FF00", 300 },
                { "#0000FF", 200 }
            };
            var result = _colorService.CalculateColorProportions(colorPixelUsage);
            Assert.That(result["#FF0000"], Is.EqualTo(50.00).Within(0.01), "Red should be 50%.");
            Assert.That(result["#00FF00"], Is.EqualTo(30.00).Within(0.01), "Green should be 30%.");
            Assert.That(result["#0000FF"], Is.EqualTo(20.00).Within(0.01), "Blue should be 20%.");
        }
        [Test]
        public void ColorProportionsOneColorOneHundredPercent()
        {
            var colorPixelUsage = new Dictionary<string, int> { { "#FFFFFF", 1000 } };
            var result = _colorService.CalculateColorProportions(colorPixelUsage);
            Assert.That(result["#FFFFFF"], Is.EqualTo(100.00).Within(0.01), "Single color should take 100%.");
        }
        [Test]
        public void ColorBalanceEmpty()
        {
            var result = _colorService.CheckColorBalance(new Dictionary<string, double>());
            Assert.That(result, Is.Empty, "Should return an empty dictionary when no colors are provided.");
        }
        [Test]
        public void ColorBalanceGrouped()
        {
            var colorProportions = new Dictionary<string, double>
            {
                { "#FF5733", 40.0 },
                { "#E65230", 20.0 }, // Similar to one above
                { "#0000FF", 40.0 }
            };
            var result = _colorService.CheckColorBalance(colorProportions);
            Assert.That(result.Count, Is.EqualTo(2), "Should group similar colors together.");
            Assert.That(result.ContainsKey("#FF5733"), "The dominant red should be the grouped key.");
            Assert.That(result["#FF5733"], Is.EqualTo(60.0).Within(0.01), "Similar red shades should be grouped.");
            Assert.That(result["#0000FF"], Is.EqualTo(40.0).Within(0.01), "Blue should remain unchanged.");
        }
        [Test]
        public void ColorBalanceNoGrouping()
        {
            var colorProportions = new Dictionary<string, double>
            {
                { "#FF0000", 40.0 },
                { "#00FF00", 30.0 },
                { "#0000FF", 30.0 }
            };
            var result = _colorService.CheckColorBalance(colorProportions);
            Assert.That(result.Count, Is.EqualTo(3), "Should not group distinct colors.");
            Assert.That(result["#FF0000"], Is.EqualTo(40.0).Within(0.01));
            Assert.That(result["#00FF00"], Is.EqualTo(30.0).Within(0.01));
            Assert.That(result["#0000FF"], Is.EqualTo(30.0).Within(0.01));
        }
        [Test]
        public void ColorBalancedEmptyInputReturnsFalse()
        {
            var result = _colorService.IsColorBalanced(new Dictionary<string, double>());
            Assert.That(result, Is.False, "Should return false when no colors are present.");
        }
        [Test]
        public void IsColorBalancedReturnsTrue()
        {
            var colorBalance = new Dictionary<string, double>
            {
                { "#FF5733", 60.0 },
                { "#B2B2B2", 30.0 },
                { "#FFFFFF", 10.0 }
            };
            var result = _colorService.IsColorBalanced(colorBalance);
            Assert.That(result, Is.True, "Should return true for a valid 60-30-10 balance.");
        }
        [Test]
        public void IsColorBalancedVarianceReturnsTrue()
        {
            var colorBalance = new Dictionary<string, double>
            {
                { "#FF5733", 70.0 },
                { "#B2B2B2", 20.0 },
                { "#FFFFFF", 5.0 }
            };
            var result = _colorService.IsColorBalanced(colorBalance);
            Assert.That(result, Is.True, "Should return true for a valid 60-30-10 balance with 15% variance.");
        }
        [Test]
        public void IsColorBalancedUnbalancedReturnsFalse()
        {
            var colorBalance = new Dictionary<string, double>
            {
                { "#E65230", 80.0 }, // Too Much
                { "#B2B2B2", 10.0 }, // Too little
                { "#FFFFFF", 5.0 }
            };
            var result = _colorService.IsColorBalanced(colorBalance);
            Assert.That(result, Is.False, "Should return false when color balance is outside of the acceptable range.");
        }
        [Test]
        public void ProtanopiaIssueTest()
        {
            Assert.That(ColorSchemeService.ProtanopiaIssue("#FF5733", "#E65230"), Is.True, "Protanopia should struggle with red shades.");
            Assert.That(ColorSchemeService.ProtanopiaIssue("#00FF00", "#0000FF"), Is.False, "Protanopia should not struggle with distinct green and blue.");
        }
        [Test]
        public void ProtanomalyIssueTest()
        {
            Assert.That(ColorSchemeService.ProtanomalyIssue("#FF5733", "#E65230"), Is.True, "Protanomaly should struggle with similar red shades.");
            Assert.That(ColorSchemeService.ProtanomalyIssue("#FFD700", "#008000"), Is.False, "Protanomaly should distinguish yellow and green.");
        }
        [Test]
        public void DeuteranopiaIssueTest()
        {
            Assert.That(ColorSchemeService.DeuteranopiaIssue("#4CAF50", "#A8E6A3"), Is.True, "Deuteranopia should struggle with green shades.");
            Assert.That(ColorSchemeService.DeuteranopiaIssue("#FF5733", "#1E90FF"), Is.False, "Deuteranopia should distinguish red and blue.");
        }

        [Test]
        public void DeuteranomalyIssueTest()
        {
            Assert.That(ColorSchemeService.DeuteranomalyIssue("#2E8B57", "#8FBC8F"), Is.True, "Deuteranomaly should struggle with green shades.");
            Assert.That(ColorSchemeService.DeuteranomalyIssue("#FFD700", "#8B0000"), Is.False, "Deuteranomaly should distinguish yellow and red.");
        }

        [Test]
        public void TritanopiaIssueTest()
        {
            Assert.That(ColorSchemeService.TritanopiaIssue("#4682B4", "#00CED1"), Is.True, "Tritanopia should struggle with blue shades.");
            Assert.That(ColorSchemeService.TritanopiaIssue("#FF4500", "#32CD32"), Is.False, "Tritanopia should distinguish red and green.");
        }

        [Test]
        public void TritanomalyIssueTest()
        {
            Assert.That(ColorSchemeService.TritanomalyIssue("#4169E1", "#87CEEB"), Is.True, "Tritanomaly should struggle with blue shades.");
            Assert.That(ColorSchemeService.TritanomalyIssue("#8B0000", "#FFFF00"), Is.False, "Tritanomaly should distinguish dark red and yellow.");
        }
        [Test]
        public void AchromatopsiaIssueTest()
        {
            Assert.That(ColorSchemeService.AchromatopsiaIssue("#8B0000", "#800080"), Is.True, "Achromatopsia should struggle with similar luminosity colors.");
            Assert.That(ColorSchemeService.AchromatopsiaIssue("#FFFFFF", "#000000"), Is.False, "Achromatopsia should distinguish black and white.");
        }
        [Test]
        public void CheckLegibilityLowContrastIssue()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "tag_colors", new Dictionary<string, string> { { "p", "#AAAAAA" } } }, // Light gray text
                { "class_colors", new Dictionary<string, string>() }, // No class-specific colors
                { "background_color", "#BBBBBB" } // Slightly different gray
            };
            var issues = _colorService.CheckLegibility(extractedData);
            Assert.That(issues, Does.Contain("Low contrast in <p>: #AAAAAA on #BBBBBB"));
        }
        [Test]
        public void CheckLegibilityProtanopiaIssueAdd()
        {
            // Adds protanopia color issue
            var extractedData = new Dictionary<string, object>
            {
                { "tag_colors", new Dictionary<string, string> { { "h1", "#FF5733" } } }, // Red text
                { "class_colors", new Dictionary<string, string>() },
                { "background_color", "#E65230" }
            };
            var issues = _colorService.CheckLegibility(extractedData);
            Assert.That(issues, Does.Contain("Protanopia issue in <h1>: #FF5733 on #E65230"));
        }
        [Test]
        public void CheckLegibilityNoIssues()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "tag_colors", new Dictionary<string, string> { { "button", "#000000" } } }, // Black text
                { "class_colors", new Dictionary<string, string>() },
                { "background_color", "#FFFFFF" }
            };
            var issues = _colorService.CheckLegibility(extractedData);
            Assert.That(issues, Is.Empty);
        }
        [Test]
        public void CheckLegibilityMultipleIssues()
        {
            var extractedData = new Dictionary<string, object>
            {
                { "tag_colors", new Dictionary<string, string> { { "span", "#CCCCCC" } } }, 
                { "class_colors", new Dictionary<string, string>() }, 
                { "background_color", "#DDDDDD" }
            };
            var issues = _colorService.CheckLegibility(extractedData);
            Assert.That(issues, Does.Contain("Low contrast in <span>: #CCCCCC on #DDDDDD"));
        }
        [Test]
        public void ExtractColorPixelsCorrectCounts(){
            byte[] testImage = GenerateTestImage();
            var result = _colorService.ExtractColorPixels(testImage);
            Assert.That(result, Is.Not.Empty);
            Assert.That(result.ContainsKey("#FF0000"), Is.True); // Red
            Assert.That(result.ContainsKey("#00FF00"), Is.True); // Green
        }
        [Test]
        public void CalculateColorProportionsFromPixelsProportions()
        {
            var colorCounts = new Dictionary<string, int>
            {
                { "#FF0000", 9800 }, // 98%
                { "#00FF00", 200 }   // 2%
            };
            var result = _colorService.CalculateColorProportionsFromPixels(colorCounts);
            Assert.That(result["#FF0000"], Is.EqualTo(98.0).Within(0.1));
            Assert.That(result["#00FF00"], Is.EqualTo(2.0).Within(0.1));
            Assert.That(result.ContainsKey("Other"), Is.False, "Other should not be present when only two colors exist.");
        }
        [Test]
        public async Task AnalyzeWebsiteColorsFromScreenshotAsyncInvalidScreenshotError()
        {
            var screenshotServiceMock = new Mock<IScreenshotService>();
            screenshotServiceMock.Setup(s => s.CaptureFullPageScreenshot(It.IsAny<string>())).ReturnsAsync(new byte[0]);
            var result = _colorService.AnalyzeWebsiteColorsFromScreenshot(new byte[0]);
            Assert.That(result.ContainsKey("colorProportions"), Is.True);
            Assert.That((bool)result["isBalanced"], Is.False);
        }
        private byte[] GenerateTestImage()
        {
            using var bitmap = new SkiaSharp.SKBitmap(100, 100);
            using var canvas = new SkiaSharp.SKCanvas(bitmap);
            using var paint = new SkiaSharp.SKPaint();
            // Red Part
            paint.Color = new SkiaSharp.SKColor(255, 0, 0);
            canvas.DrawRect(0, 0, 50, 100, paint);
            // Green Part
            paint.Color = new SkiaSharp.SKColor(0, 255, 0);
            canvas.DrawRect(50, 0, 50, 100, paint);
            using var image = SkiaSharp.SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
