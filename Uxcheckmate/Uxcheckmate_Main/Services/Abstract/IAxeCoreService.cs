using Uxcheckmate_Main.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Services
{
  public interface IAxeCoreService
  {
    // This method is responsible for performing an accessibility analysisn on a given URL.
    // Using Axe-core withing a headless browser powered by Playwright and saving the results to the database.
    Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report, CancellationToken cancellationToken = default);
  }
}