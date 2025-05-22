using System.Text;
using Microsoft.Playwright;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class MobileResponsivenessService : IMobileResponsivenessService
    {
        public MobileResponsivenessService()
        {
        }

        public async Task<string> RunMobileAnalysisAsync(string url, Dictionary<string, object> mergedData)
        {
            // StringBuilder for accumulating issues only
            var sb = new StringBuilder();

            // Attempt to extract the relevant values from the merged data
            if (mergedData.TryGetValue("viewportLabel", out var labelObj) &&
                mergedData.TryGetValue("viewportWidth", out var viewportWidthObj) &&
                mergedData.TryGetValue("scrollWidth", out var scrollWidthObj))
            {
                // Convert the viewport label to a readable string
                string label = labelObj?.ToString() ?? "Unknown viewport";

                // Parse numerical values for comparison
                if (double.TryParse(viewportWidthObj?.ToString(), out var viewportWidth) &&
                    double.TryParse(scrollWidthObj?.ToString(), out var scrollWidth))
                {
                    // Only return a report if horizontal scroll is detected
                    if (scrollWidth > viewportWidth)
                    {
                        sb.AppendLine("Mobile Responsiveness Report:\n");
                        sb.AppendLine($"{label}: Horizontal scroll detected (ScrollWidth: {scrollWidth}, ViewportWidth: {viewportWidth})");
                    }
                }
            }

            // Return the report string if there were issues, otherwise return an empty string
            return await Task.FromResult(sb.Length > 0 ? sb.ToString() : string.Empty);
        }
    }
}
