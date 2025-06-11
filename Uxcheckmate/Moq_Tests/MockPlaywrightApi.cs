using Uxcheckmate_Main.Services;
using Uxcheckmate_Main.DTO;
using Uxcheckmate_Main.Models;

public class MockPlaywrightApiService : IPlaywrightApiService
{
    public Task<PlaywrightAnalysisResult?> AnalyzeWebsiteAsync(string url, bool fullPage = false)
    {
        return Task.FromResult<PlaywrightAnalysisResult?>(new PlaywrightAnalysisResult
        {
            ScreenshotBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAUA", // fake base64 PNG
            Html = "<html><body><h1>Mock</h1><p>Paragraph</p></body></html>",
            TextContent = "Mock Paragraph",
            Fonts = new List<string> { "Arial", "Roboto" },
            HasFavicon = true,
            FaviconUrl = "https://example.com/favicon.ico",

            InlineCssList = new List<string> { "body { background: white; }" },
            InlineJsList = new List<string> { "console.log('test');" },
            ExternalCssLinks = new List<string> { "https://example.com/style.css" },
            ExternalJsLinks = new List<string> { "https://example.com/script.js" },
            ExternalCssContents = new List<string> { "body { font-size: 16px; }" },
            ExternalJsContents = new List<string> { "function test() {}" },

            ScrollHeight = 3000,
            ScrollWidth = 1200,
            ViewportHeight = 800,
            ViewportWidth = 1200,
            ViewportLabel = "1200x800",
            Links = new List<string> { "https://example.com/about" },

            LayoutElements = new List<HtmlElement>
            {
                new HtmlElement
                {
                    Tag = "H1",
                    X = 20,
                    Y = 40,
                    Width = 300,
                    Height = 50,
                    Text = "Mock Header"
                }
            },

            AxeResults = new AxeResults
            {
                Violations = new List<Uxcheckmate_Main.Services.AxeViolation>
                {
                    new Uxcheckmate_Main.Services.AxeViolation
                    {
                        Id = "color-contrast",
                        Description = "Ensure sufficient color contrast",
                        Impact = "serious",
                        Help = "Elements must have sufficient color contrast",
                        Nodes = new List<Uxcheckmate_Main.Services.AxeNode>
                        {
                            new Uxcheckmate_Main.Services.AxeNode
                            {
                                Html = "<div>Test</div>",
                                FailureSummary = "Element has insufficient contrast ratio.",
                                Target = new List<string> { "div" }
                            }
                        }
                    }
                }
            }
        });
    }
}

public class FailingPlaywrightApiService : IPlaywrightApiService
{
    public Task<PlaywrightAnalysisResult?> AnalyzeWebsiteAsync(string url, bool fullPage = false)
    {
        throw new Exception("Simulated failure");
    }
}

