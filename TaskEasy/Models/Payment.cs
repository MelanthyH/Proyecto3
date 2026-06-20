using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public decimal Monto { get; set; } = 4999; // ej: precio del plan Pro

        // "Tarjeta" o "Paypal"
        public string Metodo { get; set; } = "Tarjeta";

        // "Aprobado" o "Rechazado"
        public string Estado { get; set; } = "Aprobado";

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}