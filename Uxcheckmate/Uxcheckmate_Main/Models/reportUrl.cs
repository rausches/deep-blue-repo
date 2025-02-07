using System.ComponentModel.DataAnnotations;

namespace Uxcheckmate_Main.Models;

public class ReportUrl
{
    [Required(ErrorMessage = "Please enter a URL")]
    public string? Url { get; set; }
}