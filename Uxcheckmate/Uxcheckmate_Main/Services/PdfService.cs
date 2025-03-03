using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class PdfService
    {
        public async Task<byte[]> GenerateReportPdfAsync(ICollection<DesignIssue> issues, string url)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdfDocument = new PdfDocument(writer);
            var document = new Document(pdfDocument);

            // Use a built-in bold font
            PdfFont boldFont = PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
            
            // Title with bold font
            document.Add(new Paragraph($"UX Report for {url}")
                .SetFont(boldFont)
                .SetFontSize(18));

            if (issues.Count == 0)
            {
                document.Add(new Paragraph("No issues found."));
            }
            else
            {
                foreach (var issue in issues)
                {
                    document.Add(new Paragraph("Category: " + issue.CategoryId).SetFont(boldFont));
                    document.Add(new Paragraph("Severity: " + issue.Severity));
                    document.Add(new Paragraph("Message: " + issue.Message));
                    document.Add(new Paragraph("------------------------------"));
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
}
