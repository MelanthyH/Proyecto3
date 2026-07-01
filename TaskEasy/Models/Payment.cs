using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
        
        public decimal Monto { get; set; } = 4999;

        public string Metodo { get; set; } = "Tarjeta";

        public string Estado { get; set; } = "Aprobado";

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}