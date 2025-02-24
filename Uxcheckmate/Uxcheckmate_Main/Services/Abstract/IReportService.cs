using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public interface IReportService
    {
        Task<ICollection<DesignIssue>> GenerateReportAsync(Report report);
    }
}