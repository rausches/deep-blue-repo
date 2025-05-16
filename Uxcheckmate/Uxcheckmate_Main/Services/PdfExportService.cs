using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Uxcheckmate_Main.Models;
using System.Collections.Generic;
using System.IO;

namespace Uxcheckmate_Main.Services
{
    public class PdfExportService
    {
        public byte[] GenerateReportPdf(Report report)
        {
            string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "uxCheckmateLogo_blackandwhite.png");
            string backgroundImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "pdf_bg.PNG");

            return Document.Create(container =>
            // var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(12));

                    // Set background image
                    page.Background().Image(backgroundImagePath);

                    // Unified Header
                    page.Header().Column(headerCol =>
                    {
                        // First Row: Website & Report Date + Logo
                        headerCol.Item().Row(headerRow =>
                        {
                            // Left-aligned Website & Report Date
                            headerRow.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Website: {report.Url}")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Grey.Darken2);

                                col.Item().Text($"Report Date: {report.Date.ToString("MMM dd yyyy")}")
                                    .FontSize(10)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                            // Right-aligned Logo (if exists)
                            if (File.Exists(logoPath))
                            {
                                headerRow.ConstantItem(60).AlignRight().Height(50).Image(logoPath, ImageScaling.FitArea);
                            }
                        });

                        // Second Row: Centered Report Title
                        headerCol.Item().AlignCenter().Text("UxCheckmate Report")
                            .Bold()
                            .FontSize(30)
                            .FontColor("#001521");
                    });
                    // Main Content Section 
                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        // Design Issues Section
                        col.Item().Container().Padding(10).Column(section =>
                        {
                            // Section Title
                            section.Item().Padding(10).Column(innerSection =>
                            {
                                innerSection.Item().Text("Design Issues").Bold().FontSize(24).FontColor("#052c65");
                                innerSection.Item().Text($"Total Issues Found: {report.DesignIssues?.Count ?? 0}")
                                    .FontSize(12)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                            if (report.DesignIssues?.Count > 0)
                            {
                                foreach (var issue in report.DesignIssues)
                                {
                                section.Item().Container().Padding(10).Column(issueCol =>
                                {
                                    // First row: Category (left) and Severity (right)
                                    issueCol.Item().Container().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Category: {issue.Category.Name}")
                                            .FontSize(14)
                                            .FontColor("#052c65")
                                            .Bold();                                        

                                        row.ConstantItem(150).AlignRight().Container().Background(GetSeverityColor(issue.Severity))
                                            .Padding(5).Text($"Severity: {GetSeverityText(issue.Severity)}")
                                            .FontSize(12)
                                            .FontColor(Colors.White)
                                            .Bold();
                                    });
                                    // Second row: Description
                                    issueCol.Item().Text($"{issue.Message}")
                                        .FontSize(11);
                                });

                                }
                            }
                            else
                            {
                                section.Item().Text("No design issues found.").Italic();
                            }

                        });

                        // Accessibility Issues Section
                        col.Item().Container().Padding(10).Column(section =>
                        {
                            // Section Title
                            section.Item().Padding(10).Column(innerSection =>
                            {
                                innerSection.Item().Text("Accessibility Issues").Bold().FontSize(24).FontColor("#052c65");
                                innerSection.Item().Text($"Total Issues Found: {report.AccessibilityIssues?.Count ?? 0}")
                                    .FontSize(12)
                                    .FontColor(Colors.Grey.Darken1);
                            });

                            if (report.AccessibilityIssues?.Count > 0)
                            {
                                foreach (var issue in report.AccessibilityIssues)
                                {
                                section.Item().Container().Padding(10).Column(issueCol =>
                                {
                                    // First row: Category (left) and Severity (right)
                                    issueCol.Item().Container().BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Row(row =>
                                    {
                                        row.RelativeItem().Text($"Category: {issue.Category.Name}")
                                            .FontSize(14)
                                            .FontColor("#052c65")
                                            .Bold();                                        

                                        row.ConstantItem(150).AlignRight().Container().Background(GetSeverityColor(issue.Severity))
                                            .Padding(5).Text($"Severity: {GetSeverityText(issue.Severity)}")
                                            .FontSize(12)
                                            .FontColor(Colors.White)
                                            .Bold();
                                    });
                                    // Second row: Description
                                    issueCol.Item().Text($"{issue.Message}")
                                        .FontSize(11);
                                });

                                }
                            }
                            else
                            {
                                section.Item().Text("No design issues found.").Italic();
                            }
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Generated by UxCheckmate - {System.DateTime.Now.ToString("MMM dd yyyy")}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf();
        }

        private string GetSeverityColor(object severity)
        {
            return severity switch
            {
                1 => "#0CC5EA", // Low severity (hex color for rgb(12, 197, 234))
                2 => "#FFC107", // Moderate severity
                3 => "#DC3545", // Severe severity
                4 => "#212529", // Critical or very low severity

                string s when s.Equals("Critical", StringComparison.OrdinalIgnoreCase) => "#212529",
                string s when s.Equals("Severe", StringComparison.OrdinalIgnoreCase) => "#DC3545",
                string s when s.Equals("Moderate", StringComparison.OrdinalIgnoreCase) => "#FFC107",
                string s when s.Equals("Low", StringComparison.OrdinalIgnoreCase) => "#0CC5EA",
            };
        }

        private string GetSeverityText(object severity)
        {
            return severity switch
            {
                1 => "Low",
                2 => "Moderate",
                3 => "Severe",
                4 => "Critical",
            };
        }
    }
}