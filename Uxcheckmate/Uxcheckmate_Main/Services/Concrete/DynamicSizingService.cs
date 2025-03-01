using System.Net.Http;
using System.Threading.Tasks;

namespace Uxcheckmate_Main.Services
{
    public class DynamicSizingService : IDynamicSizingService
    {
            // Check for dynamic sizing elements in CSS
            public bool HasDynamicSizing(string htmlContent)
            {
                // Simple checks for responsive design elements
                bool hasViewportMetaTag = htmlContent.Contains("<meta name=\"viewport\"");
                bool hasMediaQueries = htmlContent.Contains("@media");
                bool hasFlexboxOrGrid = htmlContent.Contains("display: flex") || htmlContent.Contains("display: grid");
                bool hasMinMaxWidth = htmlContent.Contains("min-width") || htmlContent.Contains("max-width");

                // Check for proper use of dynamic sizing
                return hasViewportMetaTag && (hasMediaQueries || hasFlexboxOrGrid);
            }
    }
}
