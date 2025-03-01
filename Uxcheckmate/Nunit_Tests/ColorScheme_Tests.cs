using NUnit.Framework;
using Uxcheckmate_Main.Services;
using System;

namespace Uxcheckmate_Tests
{
    [TestFixture]
    public class ColorSchemeServiceTests
    {
        private ColorSchemeService _colorService;
        [SetUp]
        public void Setup()
        {
            _colorService = new ColorSchemeService();
        }
        [Test]
        public void MaxDifference_AreColorsSimilar_True()
        {
            // Setting Max 
            var color1 = (255, 100, 50);
            var color2 = (205, 100, 50);
            bool result = _colorService.AreColorsSimilar(color1, color2);
            Assert.IsTrue(result, "Colors should be considered similar.");
        }

        [Test]
        public void DiffrentColors_AreColorsSimilar_False()
        {
            var color1 = (255, 255, 255);
            var color2 = (0, 0, 0);
            bool result = _colorService.AreColorsSimilar(color1, color2);
            Assert.IsFalse(result, "Colors should be considered different.");
        }

        [Test]
        public void HexToRgb_ReturnsCorrectRgb()
        {
            var result = ColorSchemeService.HexToRgb("#FF5733");
            Assert.AreEqual((255, 87, 51), result, "Hex to RGB conversion failed.");
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
            Assert.AreEqual("#0000FF", classColors["blue-text"], "Class blue-text should be assigned #0000FF.");
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
            Assert.AreEqual("#FF5733", tagColors["p"], "Paragraph should be assigned #FF5733.");
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
            Assert.That(classCharacterCount.ContainsKey("red-text"), "Character count should be assigned to class.");
            Assert.AreEqual(9, classCharacterCount["red-text"], "Class 'red-text' should have 9 characters.");
            Assert.False(tagCharacterCount.ContainsKey("h1"), "h1 should not count characters because it's assigned to class.");
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
            Assert.AreEqual("#000000", tagColors["h2"], "Should default to black.");
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
            Assert.That(tagCharacterCount.ContainsKey("p"), "Paragraph should be counted.");
            Assert.AreEqual(21, tagCharacterCount["p"], "Paragraph should have 21 characters.");
        }
    }
}
