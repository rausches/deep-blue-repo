using System.ComponentModel.DataAnnotations;
namespace Uxcheckmate_Main.Models
{
    public class UserFeedback
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255, ErrorMessage = "Feedback cannot exceed 255 characters.")]
        public string Message { get; set; } = null!;

        public string? UserID { get; set; }
        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;
    }
}