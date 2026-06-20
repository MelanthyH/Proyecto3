using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models.DTOs
{
    public class ProjectDTO
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class TaskDTO
    {
        [Required]
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public DateTime? FechaLimite { get; set; }
        public int ProjectId { get; set; }
    }
}