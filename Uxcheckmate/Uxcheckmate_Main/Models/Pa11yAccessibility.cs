using System;
using TimeZoneConverter;

namespace Uxcheckmate_Main.Models
{
    public class Pa11yIssue
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Selector { get; set; }
        public DateTime Timestamp { get; set; } = ConvertToPacificTime(DateTime.UtcNow);
        private static DateTime ConvertToPacificTime(DateTime utcDateTime)
        {
            TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pacificZone);
        }
    }

    public class Pa11yResult
    {
        public List<Pa11yIssue> Issues { get; set; }
    }
}