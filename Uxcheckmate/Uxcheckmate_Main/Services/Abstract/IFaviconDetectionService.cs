using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public interface IFaviconDetectionService
    {
        Task<bool> DetectFaviconAsync(string url);
    }
}