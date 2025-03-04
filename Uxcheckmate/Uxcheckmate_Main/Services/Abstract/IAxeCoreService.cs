using Uxcheckmate_Main.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

/*namespace Uxcheckmate_Main.Services
{
    public interface IPa11yService
    {
      Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report);
    }
}*/

namespace Uxcheckmate_Main.Services
{
  public interface IAxeCoreService
  {
    Task<ICollection<AccessibilityIssue>> AnalyzeAndSaveAccessibilityReport(Report report);
  }
}