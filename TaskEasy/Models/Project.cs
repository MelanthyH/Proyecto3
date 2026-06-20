using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // FK
        public int UserId { get; set; }
        public User? User { get; set; }

        // Relación
        public List<TaskItem> Tasks { get; set; } = new();
    }
}