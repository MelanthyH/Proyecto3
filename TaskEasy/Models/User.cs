using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        [Required]
        public string Plan { get; set; } = "Free";

        public DateTime? PlanExpira { get; set; }

        public List<Project> Projects { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
    }
}