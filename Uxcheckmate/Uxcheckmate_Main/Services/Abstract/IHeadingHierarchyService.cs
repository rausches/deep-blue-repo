using Uxcheckmate_Main.Models;
public interface IHeadingHierarchyService
{
    Task<string> AnalyzeAsync(Dictionary<string, object> scrapedData);
}