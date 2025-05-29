using NUnit.Framework;
using QuestPDF.Infrastructure;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;
using System;


/**
namespace Service_Tests
{
    [TestFixture]
    public class PdfExportServiceTests
    {
        [SetUp]
        public void Setup()
        {
            QuestPDF.Settings.License = LicenseType.Community; // Configure license for tests
        }

        [Test]
        public void GenerateReportPdf_ShouldReturnNonEmptyByteArray()
        {
            // Arrange
            var pdfService = new PdfExportService();
            var sampleReport = new Report
            {
                Url = "https://example.com",
                Date = DateOnly.FromDateTime(DateTime.UtcNow), // Correct conversion
                AccessibilityIssues = null, // No accessibility issues for this test
                DesignIssues = null // No design issues for this test
            };

            // Act
            var result = pdfService.GenerateReportPdf(sampleReport);

            // Assert
            Assert.That(result, Is.Not.Null, "The PDF byte array is null.");
            Assert.That(result.Length, Is.GreaterThan(0), "The PDF byte array is empty.");
        }
    }
}
*/