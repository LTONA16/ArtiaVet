using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    public class CitaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El veterinario es requerido")]
        public int VeterinarioID { get; set; }

        [Required(ErrorMessage = "La mascota es requerida")]
        public int MascotaID { get; set; }

        [Required(ErrorMessage = "El tipo de cita es requerido")]
        public int TipoCitaID { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es requerida")]
        public DateTime FechaCita { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El importe adicional debe ser mayor o igual a 0")]
        public decimal ImporteAdicional { get; set; } = 0;

        [MaxLength(500)]
        public string? Observaciones { get; set; }

        // Propiedades adicionales para visualización
        public string? NombreVeterinario { get; set; }
        public string? NombreMascota { get; set; }
        public string? NombreDueno { get; set; }
        public string? TipoCita { get; set; }
        public string? ColorFondo { get; set; }
        public string? HoraInicio { get; set; }
        public string? HoraFin { get; set; }
        public decimal ImporteTotal { get; set; }
    }

    public class MascotaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El tipo de animal es requerido")]
        public int TipoAnimalID { get; set; }

        [Required(ErrorMessage = "El dueño es requerido")]
        public int DueñoID { get; set; }

        [Required(ErrorMessage = "La raza es requerida")]
        [MaxLength(50)]
        public string Raza { get; set; }

        [Required(ErrorMessage = "La edad es requerida")]
        [Range(0, 50, ErrorMessage = "La edad debe estar entre 0 y 50 años")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(50)]
        public string Nombre { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }

        // Propiedades para visualización
        public string? TipoAnimal { get; set; }
        public string? NombreDueno { get; set; }
        public List<int>? AlergiasIDs { get; set; }
    }

    public class DuenoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(200)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [MaxLength(200)]
        public string Email { get; set; }

        [Required(ErrorMessage = "El número de teléfono es requerido")]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "El número de teléfono debe tener 10 dígitos")]
        public string NumeroTelefono { get; set; }
    }

    public class CitasProximasViewModel
    {
        public List<CitasPorDia> CitasPorDias { get; set; } = new List<CitasPorDia>();
    }

    public class CitasPorDia
    {
        public DateTime Fecha { get; set; }
        public string DiaSemana { get; set; }
        public string FechaFormateada { get; set; }
        public bool EsHoy { get; set; }
        public List<CitaViewModel> Citas { get; set; } = new List<CitaViewModel>();
    }
}