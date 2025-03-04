using Uxcheckmate_Main.Models;
public interface IColorSchemeService
{
    Task<string> AnalyzeWebsiteColorsAsync(string url);
}