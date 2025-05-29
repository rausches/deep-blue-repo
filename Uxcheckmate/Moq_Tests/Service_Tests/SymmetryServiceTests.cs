using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Moq;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq_Tests;

namespace Uxcheckmate_Tests
{
    public class SymmetryServiceTests
    {
        private SymmetryService _service;

        [SetUp]
        public void Setup()
        {
            var loggerMock = new Mock<ILogger<SymmetryService>>();
            _service = new SymmetryService(loggerMock.Object);
        }
        [Test]
        public async Task AnalyzeSymmetryGoodSymmetry()
        {
            var elements = new List<HtmlElement>
            {
                new TestHtmlElement(100, 100, 100, 20, 0.02) { Tag = "DIV", IsVisible = true },
                new TestHtmlElement(700, 100, 100, 20, 0.02) { Tag = "DIV", IsVisible = true },
                new TestHtmlElement(120, 400, 100, 20, 0.015) { Tag = "DIV", Text = "foo", IsVisible = true },
                new TestHtmlElement(680, 400, 100, 20, 0.015) { Tag = "DIV", Text = "bar", IsVisible = true },
            };
            var result = await _service.AnalyzeSymmetryAsync(800, 800, elements);
            Assert.That(result, Is.Empty, "Expected no warnings for a symmetrical layout.");
        }
        [Test]
        public async Task AnalyzeSymmetryAsyncBadSymmetry()
        {
            var elements = new List<HtmlElement>
            {
                new TestHtmlElement(100, 100, 100, 20, 0.03) { IsVisible = true },
                new TestHtmlElement(300, 100, 100, 20, 0.03) { Tag = "DIV", IsVisible = true },
            };
            var result = await _service.AnalyzeSymmetryAsync(800, 800, elements);
            Assert.That(result, Does.Contain("does not have good left-right symmetry"), "Expected warning for asymmetrical layout.");
        }
    }
}
