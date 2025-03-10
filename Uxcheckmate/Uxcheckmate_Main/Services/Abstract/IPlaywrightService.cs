using Microsoft.Playwright;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    // Created an Interface for PlaywrightService to be used in Nunit testing
    public interface IPlaywrightService
    {
        Task<IBrowserContext> GetBrowserContextAsync();
    }
}
