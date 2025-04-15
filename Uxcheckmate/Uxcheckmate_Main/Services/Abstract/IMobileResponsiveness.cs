using System.Text;
using Microsoft.Playwright;
using Uxcheckmate_Main.Models;


namespace Uxcheckmate_Main.Services
{
    public interface IMobileResponsivenessService
    {


        Task<string> RunMobileAnalysisAsync(string url, Dictionary<string, object> mergedData);


    }
}
