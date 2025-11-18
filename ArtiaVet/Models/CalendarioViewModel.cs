using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    // ViewModel para representar una cita en el calendario
    public class CitaCalendarioViewModel
    {
        public int Id { get; set; }
        public int VeterinarioID { get; set; }
        public string NombreVeterinario { get; set; }
        public string NombreMascota { get; set; }
        public string NombreDueno { get; set; }
        public string TipoCita { get; set; }
        public DateTime FechaCita { get; set; }
        public decimal ImporteTotal { get; set; }
        public decimal ImporteAdicional { get; set; }
        public string Observaciones { get; set; }
        
        // Propiedades calculadas para el diseño
        public string HoraInicio => FechaCita.ToString("hh:mm tt");
        public string HoraFin => FechaCita.AddHours(1).ToString("hh:mm tt");
        public int HoraActual => FechaCita.Hour;
        
        // Color asignado dinámicamente según el veterinario
        public string ColorFondo { get; set; }
        public string ColorTexto { get; set; }
    }

    // ViewModel para la vista semanal del calendario
    public class CalendarioSemanalViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public List<DiaCalendarioViewModel> Dias { get; set; } = new();
        
        public string RangoFechas => $"{FechaInicio:dd MMM} - {FechaFin:dd MMM yyyy}";
        
        // Navegación
        public DateTime SemanaAnterior => FechaInicio.AddDays(-7);
        public DateTime SemanaSiguiente => FechaInicio.AddDays(7);
    }

    // ViewModel para cada día de la semana
    public class DiaCalendarioViewModel
    {
        public DateTime Fecha { get; set; }
        public string NombreDia => Fecha.ToString("ddd").ToUpper();
        public string NumeroYMes => Fecha.ToString("dd");
        public bool EsHoy => Fecha.Date == DateTime.Today;
        public bool EsDiaLaboral { get; set; }
        public List<CitaCalendarioViewModel> Citas { get; set; } = new();
        
        // Rangos de horas laborales
        public List<int> HorasLaborales { get; set; } = new();
    }

    // ViewModel para los veterinarios con su color asignado
    public class VeterinarioCalendarioViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string ColorFondo { get; set; }
        public string ColorTexto { get; set; }
        public int TotalCitasSemana { get; set; }
    }
}