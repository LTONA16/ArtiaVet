using System.ComponentModel.DataAnnotations;
namespace ArtiaVet.Models
{
    public class RecordatorioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La cita es requerida")]
        public int CitaID { get; set; }

        [Required(ErrorMessage = "La fecha del recordatorio es requerida")]
        public DateTime FechaRecordatorio { get; set; }

        [Required(ErrorMessage = "El asunto es requerido")]
        [MaxLength(200)]
        public string Asunto { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [MaxLength(500)]
        public string Mensaje { get; set; }

        // Propiedades adicionales para visualización
        public string? NombreDueno { get; set; }
        public string? NombreMascota { get; set; }
        public string? TelefonoDueno { get; set; }
        public string? EmailDueno { get; set; }
        public string? HoraRecordatorio { get; set; }
    }

    public class RecordatoriosProximosViewModel
    {
        public List<RecordatoriosPorDia> RecordatoriosPorDias { get; set; } = new List<RecordatoriosPorDia>();
    }

    public class RecordatoriosPorDia
    {
        public DateTime Fecha { get; set; }
        public string DiaSemana { get; set; }
        public string FechaFormateada { get; set; }
        public bool EsHoy { get; set; }
        public List<RecordatorioViewModel> Recordatorios { get; set; } = new List<RecordatorioViewModel>();
    }
}