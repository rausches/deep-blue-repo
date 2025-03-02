using Uxcheckmate_Main.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Uxcheckmate_Main.Services
{
    public interface IDynamicSizingService
    {
    bool HasDynamicSizing(string htmlContent);

    }
}