using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    public class FacturaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La cita es requerida")]
        public int CitaID { get; set; }

        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El IVA es requerido")]
        [Range(0, double.MaxValue, ErrorMessage = "El IVA debe ser mayor o igual a 0")]
        public decimal Iva { get; set; }

        [Required(ErrorMessage = "El total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }

        // Propiedades adicionales para visualización
        public DateTime FechaCita { get; set; }
        public string? NombreVeterinario { get; set; }
        public string? NombreMascota { get; set; }
        public string? NombreDueno { get; set; }
        public string? TipoCita { get; set; }
        public string? FechaFormateada { get; set; }
    }

    public class FacturasViewModel
    {
        public List<FacturaViewModel> Facturas { get; set; } = new List<FacturaViewModel>();
        public string? TerminoBusqueda { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }

    public class CitaParaFacturaViewModel
    {
        public int Id { get; set; }
        public string NombreMascota { get; set; }
        public string NombreDueno { get; set; }
        public string TipoCita { get; set; }
        public string NombreVeterinario { get; set; }
        public DateTime FechaCita { get; set; }
        public decimal ImporteBase { get; set; }
        public decimal ImporteAdicional { get; set; }
        public decimal ImporteTotal { get; set; }
    }
}