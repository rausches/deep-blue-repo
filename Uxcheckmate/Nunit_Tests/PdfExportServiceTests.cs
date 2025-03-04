using NUnit.Framework;
using System;
using System.Collections.Generic;
using Uxcheckmate_Main.Models;
using Uxcheckmate_Main.Services;

namespace Uxcheckmate_Tests

{
    [TestFixture]
    public class PdfExportServiceTests
    {
        private PdfExportService _pdfExportService;

        [SetUp]
        public void Setup()
        {
            _pdfExportService = new PdfExportService();
        }

        [Test]
        public void GenerateReportPdf_ValidReport_ReturnsNonEmptyByteArray()
        {
            // Arrange
            var report = new Report
            {
                Url = "https://example.com",
                Date = DateTime.UtcNow,
                AccessibilityIssues = new List<Issue>
                {
                    new Issue { Message = "Missing alt text", Severity = "High" },
                    new Issue { Message = "Low contrast text", Severity = "Medium" }
                },
                DesignIssues = new List<Issue>
                {
                    new Issue { Message = "Inconsistent button styles", Severity = "Low" }
                }
            };

            // Act
            var pdfBytes = _pdfExportService.GenerateReportPdf(report);

            // Assert
            Assert.IsNotNull(pdfBytes, "PDF byte array should not be null.");
            Assert.IsNotEmpty(pdfBytes, "PDF byte array should not be empty.");
        }
    }
}
