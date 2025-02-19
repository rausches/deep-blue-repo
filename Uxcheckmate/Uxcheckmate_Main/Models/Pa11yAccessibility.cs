using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Uxcheckmate_Main.Models;

public class Pa11yIssue
{
     //  If the case of the JSON keys differs from your model’s property names
    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("selector")]
    public string Selector { get; set; }
    public int Severity { get; set; } = 3; // 1 is highest and 3 is lowest. So lower number means how priority
    public DateTime Timestamp { get; set; } = ConvertToPacificTime(DateTime.UtcNow);
    private static DateTime ConvertToPacificTime(DateTime utcDateTime)
        {
                TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pacificZone);
        }
}


public class Pa11yResult{
        public List<Pa11yIssue>? Issues { get; set; }
}