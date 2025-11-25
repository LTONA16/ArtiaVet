using Microsoft.Data.SqlClient;
using ArtiaVet.Models;
using System.Globalization;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioCalendarioVeterinario
    {
        Task<CalendarioSemanalViewModel> ObtenerCalendarioSemanalAsync(DateTime fecha, int veterinarioId);
        Task<CitaCalendarioViewModel> ObtenerDetalleCitaAsync(int citaId, int veterinarioId);
    }

    public class RepositorioCalendarioVeterinario : IRepositorioCalendarioVeterinario
    {
        private readonly string connectionString;
        
        // Color único para el veterinario autenticado
        private readonly (string Fondo, string Texto) _colorVeterinario = ("bg-blue-100", "text-blue-800");

        public RepositorioCalendarioVeterinario(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<CalendarioSemanalViewModel> ObtenerCalendarioSemanalAsync(DateTime fecha, int veterinarioId)
        {
            var calendario = new CalendarioSemanalViewModel();
            
            // Calcular inicio de semana (lunes)
            var diaSemana = (int)fecha.DayOfWeek;
            var diasHastaLunes = diaSemana == 0 ? -6 : -(diaSemana - 1);
            var inicioSemana = fecha.Date.AddDays(diasHastaLunes);
            
            calendario.FechaInicio = inicioSemana;
            calendario.FechaFin = inicioSemana.AddDays(6);

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        c.id,
                        c.veterinarioID,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        du.nombre AS nombreDueno,
                        tc.nombre AS tipoCita,
                        c.fechaCita,
                        tc.importe AS importeCita,
                        c.importeAdicional,
                        c.observaciones
                    FROM Citas c
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños du ON m.dueñoID = du.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.fechaCita >= @fechaInicio
                    AND c.fechaCita < @fechaFin
                    AND c.veterinarioID = @veterinarioId
                    ORDER BY c.fechaCita;";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", inicioSemana);
                command.Parameters.AddWithValue("@fechaFin", inicioSemana.AddDays(7));
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                var citas = new List<CitaCalendarioViewModel>();
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    citas.Add(new CitaCalendarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = Convert.ToInt32(reader["veterinarioID"]),
                        NombreVeterinario = reader["nombreVeterinario"].ToString(),
                        NombreMascota = reader["nombreMascota"].ToString(),
                        NombreDueno = reader["nombreDueno"].ToString(),
                        TipoCita = reader["tipoCita"].ToString(),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteTotal = Convert.ToDecimal(reader["importeCita"]) + 
                                      Convert.ToDecimal(reader["importeAdicional"]),
                        ImporteAdicional = Convert.ToDecimal(reader["importeAdicional"]),
                        Observaciones = reader["observaciones"]?.ToString() ?? "",
                        ColorFondo = _colorVeterinario.Fondo,
                        ColorTexto = _colorVeterinario.Texto
                    });
                }

                // Organizar citas por día
                calendario.Dias = OrganizarCitasPorDia(inicioSemana, citas);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener calendario semanal: {ex.Message}");
                throw;
            }

            return calendario;
        }

        public async Task<CitaCalendarioViewModel> ObtenerDetalleCitaAsync(int citaId, int veterinarioId)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                var query = @"
                    SELECT 
                        c.id,
                        c.veterinarioID,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        du.nombre AS nombreDueno,
                        tc.nombre AS tipoCita,
                        c.fechaCita,
                        tc.importe AS importeCita,
                        c.importeAdicional,
                        c.observaciones
                    FROM Citas c
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños du ON m.dueñoID = du.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.id = @citaId
                    AND c.veterinarioID = @veterinarioId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@citaId", citaId);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CitaCalendarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = Convert.ToInt32(reader["veterinarioID"]),
                        NombreVeterinario = reader["nombreVeterinario"].ToString(),
                        NombreMascota = reader["nombreMascota"].ToString(),
                        NombreDueno = reader["nombreDueno"].ToString(),
                        TipoCita = reader["tipoCita"].ToString(),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteTotal = Convert.ToDecimal(reader["importeCita"]) + 
                                      Convert.ToDecimal(reader["importeAdicional"]),
                        ImporteAdicional = Convert.ToDecimal(reader["importeAdicional"]),
                        Observaciones = reader["observaciones"]?.ToString() ?? "",
                        ColorFondo = _colorVeterinario.Fondo,
                        ColorTexto = _colorVeterinario.Texto
                    };
                }

                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener detalle de cita: {ex.Message}");
                throw;
            }
        }

        // Método privado para organizar citas por día
        private List<DiaCalendarioViewModel> OrganizarCitasPorDia(DateTime inicioSemana, List<CitaCalendarioViewModel> citas)
        {
            var dias = new List<DiaCalendarioViewModel>();
            
            for (int i = 0; i < 7; i++)
            {
                var fecha = inicioSemana.AddDays(i);
                var esDomingo = fecha.DayOfWeek == DayOfWeek.Sunday;
                var esSabado = fecha.DayOfWeek == DayOfWeek.Saturday;
                
                var dia = new DiaCalendarioViewModel
                {
                    Fecha = fecha,
                    EsDiaLaboral = !esDomingo,
                    Citas = citas.Where(c => c.FechaCita.Date == fecha.Date).ToList()
                };

                // Definir horas laborales según el día
                if (esSabado)
                {
                    dia.HorasLaborales = Enumerable.Range(9, 5).ToList(); // 9 AM - 2 PM
                }
                else if (!esDomingo)
                {
                    dia.HorasLaborales = Enumerable.Range(9, 10).ToList(); // 9 AM - 7 PM
                }

                dias.Add(dia);
            }

            return dias;
        }
    }
}