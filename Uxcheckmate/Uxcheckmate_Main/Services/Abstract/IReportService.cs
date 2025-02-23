using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IReportService
    {
        Task<List<DesignIssue>> GenerateReportAsync(string url);
    }
}