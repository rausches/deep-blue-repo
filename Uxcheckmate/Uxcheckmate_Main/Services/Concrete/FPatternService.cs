using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uxcheckmate_Main.Models;

namespace Uxcheckmate_Main.Services
{
    public class FPatternService : IFPatternService
    {
        private readonly ILogger<FPatternService> _logger;

        // [Tunable Parameters]

        // Band height as a fraction of viewport or max absolute pixels
        private readonly double _bandHeightFraction = 0.5;
        private readonly double _bandHeightMax = 800;

        // Minimum vertical gap between elements to start a new band [in px]
        private readonly double _gapThreshold = 300;

        // How far from the left is considered "left zone" [% fraction of width]
        private readonly double _leftZoneFraction = 0.27;

        // Minimum average density to not be considered a low-density band
        private readonly double _adaptiveThresholdMultiplier = 2.0;
        private readonly double _adaptiveThresholdMin = 0.015;

        // F-Pattern scoring weights % (top/middle/left zones)
        private readonly double _scoreWeightTop = 0.15;
        private readonly double _scoreWeightMiddle = 0.15;
        private readonly double _scoreWeightLeft = 0.7;

        // Minimum weight assigned to any band
        private readonly double _minBandWeight = 0.3;

        // How much to added to the average score for grace % wise
        private readonly double _scoreGraceBump = 0.3;

        // Threshold for passing F-pattern
        private readonly double _goodScoreThreshold = 0.65;

        public FPatternService(ILogger<FPatternService> logger)
        {
            _logger = logger;
        }

        public async Task<string> AnalyzeFPatternAsync(double viewportWidth, double viewportHeight, List<HtmlElement> elements)
        {
            if (elements == null || elements.Count == 0){
                _logger.LogWarning("No layout elements provided found for F analysis.");
                return ""; // If no elements do not analyze
            }
            elements = FilterElements(elements);
            foreach (var element in elements){
                _logger.LogDebug($"Element: {element.Tag}, X: {element.X}, Y: {element.Y}, Width: {element.Width}, Height: {element.Height}, Text Length: {element.Text?.Length ?? 0}");
            }

            // Calculate fixed band height based on viewport size or max limit
            double fixedBandHeight = Math.Min(viewportHeight * _bandHeightFraction, _bandHeightMax);
            double gapThreshold = _gapThreshold;
            double leftZoneThreshold = Math.Min(_leftZoneFraction * viewportWidth, 300);

            var bands = GetBands(elements, fixedBandHeight, gapThreshold);

            double globalAvgDensity = elements.Average(e => e.Density);
            double adaptiveThreshold = GetAdaptiveThreshold(globalAvgDensity);

            _logger.LogDebug($"Global Average Density: {globalAvgDensity:F4}, Adaptive Threshold: {adaptiveThreshold:F4}");

            var (sectionScores, weights) = ScoreBands(bands, fixedBandHeight, leftZoneThreshold, adaptiveThreshold);

            if (sectionScores.Count == 0){
                return "No valid sections found for F-pattern analysis.";
            }
            double generousAverage = GetWeightedAverage(sectionScores, weights);
            string summary = $"Average F-Pattern Score: {generousAverage:P0}\n";
            _logger.LogDebug($"Average F-Pattern Score: {generousAverage:P0}");
            if (generousAverage >= _goodScoreThreshold){
                summary = ""; // No warning, considered good enough
            }else{
                summary += "This layout does not follow the F-pattern well.";
            }
            return await Task.FromResult(summary);
        }

        // Splits elements into horizontal bands based on Y position and height
        private List<List<HtmlElement>> GetBands(List<HtmlElement> elements, double fixedBandHeight, double gapThreshold)
        {
            var sorted = elements.OrderBy(e => e.Y).ToList();
            List<List<HtmlElement>> bands = new();
            List<HtmlElement> currentBand = new() { sorted[0] };
            for (int i = 1; i < sorted.Count; i++){
                double gap = sorted[i].Y - (sorted[i - 1].Y + sorted[i - 1].Height);
                if (gap > gapThreshold || sorted[i].Y > currentBand[0].Y + fixedBandHeight){
                    bands.Add(currentBand);
                    _logger.LogDebug($"Band {i} has {currentBand.Count} elements: " + string.Join(", ", currentBand.Select(e => $"{e.Tag}({e.Text?.Length ?? 0}) [{e.X},{e.Y}]")));
                    currentBand = new();
                }
                currentBand.Add(sorted[i]);
            }
            if (currentBand.Count > 0){
                bands.Add(currentBand);
            }
            return bands;
        }

        // Calculateing adaptive threshold based on global average density [Finds concentration of text in a sense]
        private double GetAdaptiveThreshold(double globalAvgDensity)
        {
            return Math.Max(_adaptiveThresholdMin, globalAvgDensity * _adaptiveThresholdMultiplier);
        }

        // Takes all the bands and runs functions to run score and weight
        private (List<double> scores, List<double> weights) ScoreBands(List<List<HtmlElement>> bands, double fixedBandHeight, double leftZoneThreshold, double adaptiveThreshold)
        {
            List<double> sectionScores = new();
            List<double> weights = new();
            for (int i = 0; i < bands.Count; i++)
            {
                var band = bands[i];
                double totalDensity = band.Sum(e => e.Density);
                bool lowDensity = totalDensity < adaptiveThreshold;
                if (lowDensity){
                    _logger.LogDebug($"Low density band {i}: {totalDensity:F2} < {adaptiveThreshold:F2} — still evaluating but weighing less.");
                }
                double score = ScoreBand(band, fixedBandHeight, leftZoneThreshold, totalDensity);
                sectionScores.Add(score);
                double weight = GetBandWeight(i, lowDensity);
                weights.Add(weight);
            }
            return (sectionScores, weights);
        }
        // Calculating score for an individual band
        private double ScoreBand(List<HtmlElement> band, double fixedBandHeight, double leftZoneThreshold, double totalDensity)
        {
            // Nothing in it return 0
            if (totalDensity == 0){
                return 0;
            }

            // Figuring out density zones with elements
            double minY = band.Min(b => b.Y);
            double topBoundary = minY + 0.2 * fixedBandHeight;
            double middleBoundary = minY + 0.4 * fixedBandHeight;
            double topZoneDensity = band.Where(e => e.Y < topBoundary).Sum(e => e.Density);
            double middleZoneDensity = band.Where(e => e.Y >= topBoundary && e.Y < middleBoundary).Sum(e => e.Density);
            double leftZoneDensity = band.Where(e => e.X < leftZoneThreshold).Sum(e => e.Density);


            // Scoring based on weighted zones
            double score = (_scoreWeightTop * topZoneDensity + _scoreWeightMiddle * middleZoneDensity + _scoreWeightLeft * leftZoneDensity) / totalDensity;
            score = Math.Round(score, 2);

            // Logging the score breakdown
            _logger.LogDebug($"Band Score => Top: {topZoneDensity:F2}, Middle: {middleZoneDensity:F2}, Left: {leftZoneDensity:F2}, Total: {totalDensity:F2}, Score: {score:F2}");
            return score;
        }
        // Determines the weight for an individual band (lower for low-density bands, heavier for top bands)
        private double GetBandWeight(int bandIndex, bool lowDensity)
        {
            double baseWeight = Math.Max(_minBandWeight, 1.0 / (bandIndex + 1));
            return lowDensity ? 0.5 * baseWeight : baseWeight;
        }
        // Computes the weighted average score for the F-pattern and applies grace bump
        private double GetWeightedAverage(List<double> scores, List<double> weights)
        {
            double weightedAverage = scores.Zip(weights, (score, w) => score * w).Sum() / weights.Sum();
            return Math.Round(Math.Min(1.0, weightedAverage + _scoreGraceBump), 2);
        }
        // Filtering elements based on visibility, if it's small, and if it has text
        private List<HtmlElement> FilterElements(List<HtmlElement> elements)
        {
            return elements.Where(e =>
                e.Width > 10 &&
                e.Height > 10 &&
                e.X >= 0 && e.Y >= 0 &&
                !string.IsNullOrWhiteSpace(e.Text) &&
                e.Text.Trim().Length > 1 &&
                !IsSidebarOrNav(e) &&
                !(e.Tag.Equals("A", StringComparison.OrdinalIgnoreCase) && e.Text.Trim().Length < 6)
            ).ToList();
        }

        // Checking if Nav or sidebar
        private bool IsSidebarOrNav(HtmlElement e)
        {
            return e.Tag.Equals("NAV", StringComparison.OrdinalIgnoreCase)
                || e.Tag.Equals("ASIDE", StringComparison.OrdinalIgnoreCase)
                || (e.Class?.ToLower().Contains("sidebar") ?? false)
                || (e.Id?.ToLower().Contains("sidebar") ?? false)
                || (e.Class?.ToLower().Contains("toc") ?? false)
                || (e.Id?.ToLower().Contains("toc") ?? false);
        }

    }
}
