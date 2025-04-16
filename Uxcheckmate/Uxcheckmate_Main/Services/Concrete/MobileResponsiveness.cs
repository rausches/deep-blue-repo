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
            // StringBuilder for accumulating formatted results
            var sb = new StringBuilder();
            sb.AppendLine("Mobile Responsiveness Report:\n");

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
                    // Check if horizontal scrolling would occur
                    if (scrollWidth > viewportWidth)
                    {
                        sb.AppendLine($"{label}: Horizontal scroll detected at (ScrollWidth: {scrollWidth}, ViewportWidth: {viewportWidth})");
                    }
                    else
                    {
                        sb.AppendLine($"{label}: Layout fits screen (ViewportWidth: {viewportWidth})");
                    }
                }
                else
                {
                    // Handle case where numeric parsing failed
                    sb.AppendLine("Could not parse scroll/viewport width values.");
                }
            }
            else
            {
                // Handle missing required fields from mergedData
                sb.AppendLine("Required data for responsiveness analysis not found.");
            }

            // Return the report as a string
            return await Task.FromResult(sb.ToString());
        }
    }
}
