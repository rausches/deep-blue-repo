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
    }
}
