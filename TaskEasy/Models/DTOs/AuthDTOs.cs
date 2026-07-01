using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models.DTOs
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(4)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AdminUserEditDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        [Required]
        public string Plan { get; set; } = "Free";
    }
}