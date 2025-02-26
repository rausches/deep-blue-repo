using System.IO;
using System.Threading.Tasks;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Uxcheckmate_Main.Services
{
    public class PdfService
    {
        private readonly WebScraperService _scraperService;

        public PdfService(WebScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        public async Task<byte[]> GeneratePdfAsync(string url)
        {
            var extractedData = await _scraperService.ScrapeAsync(url);

            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var font = new XFont("Arial", 12, XFontStyle.Regular);

                gfx.DrawString($"UX Report for {url}", font, XBrushes.Black, new XRect(20, 20, page.Width, page.Height), XStringFormats.TopLeft);

                int yOffset = 60;
                foreach (var item in extractedData)
                {
                    gfx.DrawString($"{item.Key}: {item.Value}", font, XBrushes.Black, new XRect(20, yOffset, page.Width, page.Height), XStringFormats.TopLeft);
                    yOffset += 20;
                }

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }
    }
}
