using System.ComponentModel.DataAnnotations;

namespace Uxcheckmate_Main.Models;

public class ReportUrl
{
    [Required(ErrorMessage = "The URL field is required.")]
    [Url(ErrorMessage = "The URL is not valid.")]
    public string? Url { get; set; }
}