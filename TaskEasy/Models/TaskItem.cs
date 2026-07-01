using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Required]
        public string Estado { get; set; } = "Pendiente";

        public DateTime? FechaLimite { get; set; }

        // FK
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}