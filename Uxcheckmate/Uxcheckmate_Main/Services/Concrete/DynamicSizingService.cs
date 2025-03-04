using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace Uxcheckmate_Main.Services
{
    public class DynamicSizingService : IDynamicSizingService
    {
            // Check for dynamic sizing elements in CSS
            public bool HasDynamicSizing(string htmlContent)
            {
                // Simple checks for responsive design elements
                // Using Regex to check for the <meta name="viewport"> tag, ensuring case insensitivity and handling variations in whitespace and attribute order
                bool hasViewportMetaTag = Regex.IsMatch(htmlContent, @"<meta\s+name\s*=\s*['""]viewport['""]", RegexOptions.IgnoreCase);
                bool hasMediaQueries = htmlContent.Contains("@media");
                bool hasFlexboxOrGrid = htmlContent.Contains("display: flex") || htmlContent.Contains("display: grid");
                bool hasMinMaxWidth = htmlContent.Contains("min-width") || htmlContent.Contains("max-width");

                // Check for proper use of dynamic sizing
                return hasViewportMetaTag && (hasMediaQueries || hasFlexboxOrGrid);
            }
    }
}
