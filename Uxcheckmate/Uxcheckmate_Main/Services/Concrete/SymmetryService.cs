using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

public class SymmetryService : ISymmetryService
{
    // Constants for symmetry analysis
    private readonly double _centerToleranceFraction = 0.12; // Middle Viewport width %
    private readonly double _minGoodScore = 0.8; // Minimum % score for good symmetry 
    // Percentage of score for the different elements
    private readonly double _centerWeight = 0.1; // % Of center weight contributing to score
    private readonly double _leftRightWeight;
    // Center dominance checks [Things to check if center is dominant]
    private readonly double _centerDominanceMultiplier = 2.0;   // How much more must center be than left/right
    private readonly double _centerDominanceFraction = 0.7;     // How much of total denisity much center take up
    private readonly double _centerDominantBaseScore = 0.85;    // Minimum score when center is dominant
    private readonly double _centerDominantBonusMultiplier = 0.1; // Bonus for how much centerFraction exceeds threshold

    private readonly ILogger<SymmetryService> _logger;

    public SymmetryService(ILogger<SymmetryService> logger)
    {
        _logger = logger;
        _leftRightWeight = 1 - _centerWeight;
    }

    public async Task<string> AnalyzeSymmetryAsync(double viewportWidth, double viewportHeight, List<HtmlElement> elements)
    {
        // Setting elements to only visible ones
        elements = elements.Where(e => e.IsVisible && IsVisualContent(e)).ToList();

        // Checking if there are elements to analyze
        if (elements == null || elements.Count == 0)
        {
            _logger.LogWarning("No layout elements found for symmetry analysis.");
            return ""; // Does not report anything if no elements are present
        }

        double centerX = viewportWidth / 2; // Find center of the viewport
        double centerTolerance = _centerToleranceFraction * viewportWidth; // Calculate tolerance of middle area

        // Break things apart into density groups [How much elements take up space on sides]
        var (leftElements, rightElements, centerElements) = SplitElements(elements, centerX, centerTolerance);
        double leftDensity = SumDensity(leftElements);
        double rightDensity = SumDensity(rightElements);
        double centerDensity = SumDensity(centerElements);

        // Debug for checking densities
        _logger.LogDebug($"Center Density: {centerDensity:F4}");
        _logger.LogDebug($"Left Density: {leftDensity:F4}, Right Density: {rightDensity:F4}");

        // calculate total density
        double totalDensity = leftDensity + rightDensity + centerDensity;
        if (totalDensity == 0)
        {
            return ""; // If no density do not report anything
        }

        // Calculate score
        double symmetryScore = CalculateSymmetryScore(leftDensity, rightDensity, centerDensity, totalDensity);
        string summary = FormatSymmetryResult(symmetryScore);

        // Return score if problem if not just logging it for debugging
        if (symmetryScore >= _minGoodScore)
        {
            _logger.LogDebug($"Good symmetry detected (Score: {symmetryScore:P0})");
            return "";
        }
        else
        {
            _logger.LogDebug($"Low symmetry detected (Score: {symmetryScore:P0})");
            summary += "This layout does not have good left-right symmetry.";
            return await Task.FromResult(summary);
        }
    }

    // Spliting elements into left, right, and center based on their X position and tolerance
    private (List<HtmlElement> left, List<HtmlElement> right, List<HtmlElement> center) SplitElements(
        List<HtmlElement> elements, double centerX, double centerTolerance)
    {
        var left = elements.Where(e => e.X + e.Width / 2 < centerX - centerTolerance).ToList();
        var right = elements.Where(e => e.X + e.Width / 2 > centerX + centerTolerance).ToList();
        var center = elements.Where(e => Math.Abs((e.X + e.Width / 2) - centerX) <= centerTolerance).ToList();
        return (left, right, center);
    }

    // Calculating the sum of densities
    private double SumDensity(List<HtmlElement> elements)
    {
        return elements.Sum(e => e.Density);
    }

    // Giving a score based on how close left and right densities are and how much is in the center
    private double CalculateSymmetryScore(double left, double right, double center, double total)
    {
        double leftRightSum = left + right;
        double leftRightScore = leftRightSum > 0 ? 1.0 - Math.Abs(left - right) / leftRightSum : 1.0;
        // If left and right score is good return it
        if (leftRightScore >= 0.8)
        {
            return Math.Round(leftRightScore, 2);
        }
        // Check to see if center is dominant
        double centerFraction = center / total;
        bool centerDominates = center > _centerDominanceMultiplier * leftRightSum && centerFraction > _centerDominanceFraction;
        if (centerDominates)
        {
            return Math.Round(_centerDominantBaseScore + _centerDominantBonusMultiplier * (centerFraction - _centerDominanceFraction), 2);
        }
        double blendedScore = _centerWeight * centerFraction + _leftRightWeight * leftRightScore;
        return Math.Round(blendedScore, 2);
    }


    // Making symmetry score a readable string
    private string FormatSymmetryResult(double score)
    {
        return $"Symmetry Score: {score:P0}\n";
    }
    
    private bool IsVisualContent(HtmlElement e)
    {
        if (e.Tag == "NAV" || e.Tag == "FOOTER"){
            return false; // Content in nav and footer not counted
        }else if ((e.Width > 50 && e.Height > 50) && (e.Tag == "DIV" || e.Tag == "IMG" || e.Tag == "SECTION" || e.Tag == "ARTICLE")){
            return true; // if proper size and tag, count as visual content
        }else if (!string.IsNullOrWhiteSpace(e.Text) && e.Width > 1 && e.Height > 1){
            return true; // Check if element has text and is not too small
        }else{
            return false; // Does not meet criteria so not visual content
        }
    }

}