namespace ArtiaVet.Models
{
    public class CitaCalendarioViewModel
    {
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public string NombreVeterinario { get; set; }
        public string NombreMascota { get; set; }
        public string TipoCita { get; set; }
        public decimal ImporteAdicional { get; set; }
        public string Observaciones { get; set; }
    }

    public class CitaDetalleViewModel : CitaCalendarioViewModel
    {
        public string NombreDueño { get; set; }
        public string TelefonoDueño { get; set; }
    }

}
