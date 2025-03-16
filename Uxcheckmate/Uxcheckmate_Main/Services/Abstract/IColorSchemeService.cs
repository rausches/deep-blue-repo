using Uxcheckmate_Main.Models;

public interface IColorSchemeService
{
    Task<string> AnalyzeWebsiteColorsAsync(Dictionary<string, object> scrapedData, Task<byte[]> screenshotTask);
}