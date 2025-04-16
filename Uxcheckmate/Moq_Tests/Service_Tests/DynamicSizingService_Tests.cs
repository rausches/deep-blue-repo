using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Service_Tests
{
    [TestFixture]
    public class DynamicSizingServiceTests
    {
        private DynamicSizingService _dynamicSizingService;

        [SetUp]
        public void Setup()
        {
            _dynamicSizingService = new DynamicSizingService();
        }

        [Test]
        public void HasDynamicSizing_WhenHtmlContainsResponsiveElements_ReturnsTrue()
        {
            // Arrange: HTML with dynamic sizing elements
            string htmlContent = @"
                <html>
                <head>
                    <meta name='viewport' content='width=device-width, initial-scale=1'>
                    <style>
                        @media (max-width: 600px) {
                            body { background-color: lightblue; }
                        }
                        .container { display: flex; }
                    </style>
                </head>
                <body>
                </body>
                </html>"; 

            // Act
            bool result = _dynamicSizingService.HasDynamicSizing(htmlContent); 

            // Assert
            Assert.That(result, Is.True);  
        }

       [Test]
        public void HasDynamicSizing_WhenHtmlDoesNotContainResponsiveElements_ReturnsFalse()
        {
            // Arrange: HTML without dynamic sizing elements
            string htmlContent = @"
                <html>
                <head>
                    <style>
                        .container { width: 800px; }
                    </style>
                </head>
                <body>
                </body>
                </html>";

            // Act
            bool result = _dynamicSizingService.HasDynamicSizing(htmlContent);  

            // Assert
            Assert.That(result, Is.False); 
        }
    }
}