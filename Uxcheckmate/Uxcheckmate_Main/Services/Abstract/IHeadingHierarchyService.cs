using Uxcheckmate_Main.Models;
public interface IHeadingHierarchyService
{
    Task<string> AnalyzeAsync(string url);
}