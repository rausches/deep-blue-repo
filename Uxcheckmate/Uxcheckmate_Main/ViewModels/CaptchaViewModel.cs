using System.ComponentModel.DataAnnotations;
public class CaptchaViewModel
{
    public string Url { get; set; } = string.Empty;
    public string SortOrder { get; set; } = "category";
    [Required]
    [Display(Name = "Captcha")]
    public string CaptchaToken { get; set; } = string.Empty;
}
