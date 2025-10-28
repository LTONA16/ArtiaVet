using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    public class CitaViewModel
    {
        [Required(ErrorMessage = "El veterinario es requerido")]
        public int VeterinarioID { get; set; }

        [Required(ErrorMessage = "La mascota es requerida")]
        public int MascotaID { get; set; }

        [Required(ErrorMessage = "El tipo de cita es requerido")]
        public int TipoCitaID { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime FechaCita { get; set; }

        public decimal ImporteAdicional { get; set; } = 0;

        [MaxLength(500)]
        public string? Observaciones { get; set; }
    }
}