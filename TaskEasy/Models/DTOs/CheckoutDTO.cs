using System.ComponentModel.DataAnnotations;

namespace TaskEasy.Models.DTOs
{
    public class CheckoutDTO
    {
        [Required]
        public string Metodo { get; set; } = "Paypal"; 

        [Required]
        [StringLength(16, MinimumLength = 16)]
        public string NumeroTarjeta { get; set; } = string.Empty;

        [Required]
        public string NombreTitular { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CVV { get; set; } = string.Empty;
    }
}