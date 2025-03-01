using System;
using System.Globalization;

namespace Uxcheckmate_Main.Services
{
    public class ColorSchemeService : IColorSchemeService
    {
        public bool AreColorsSimilar((int R, int G, int B) color1, (int R, int G, int B) color2, int threshold = 50)
        {
            //Grabbing differences
            int diffR = color1.R - color2.R;
            int diffG = color1.G - color2.G;
            int diffB = color1.B - color2.B;
            //Grabbing overall difference
            double distance = Math.Sqrt(diffR * diffR + diffG * diffG + diffB * diffB);
            return distance < threshold;
        }
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            // Removing the hashtag
            if (hex.StartsWith("#")){
                hex = hex.Substring(1);
            }
            // Taking hex and converting to RGB
            if (hex.Length == 6){
                int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
                return (r, g, b);
            }else{
                throw new ArgumentException("Invalid hex color format. Use #RRGGBB or RRGGBB.");
            }
        }
    }
}
