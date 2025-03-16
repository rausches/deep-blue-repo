using Uxcheckmate_Main.Models;
using System.Threading.Tasks;
using Microsoft.Playwright;


namespace Uxcheckmate_Main.Services
{
    public interface IScreenshotService
    {
        // Method to take a screenshot of the webpage 
        Task<string> CaptureScreenshot(PageScreenshotOptions screenshotOptions, string url);
        Task<byte[]> CaptureFullPageScreenshot(string url);
    }
}