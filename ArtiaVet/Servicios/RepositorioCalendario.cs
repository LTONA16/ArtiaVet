using Microsoft.Data.SqlClient;
using ArtiaVet.Models;
using System.Globalization;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioCalendario
    {
        Task<CalendarioSemanalViewModel> ObtenerCalendarioSemanalAsync(DateTime fecha);
        Task<CitaCalendarioViewModel> ObtenerDetalleCitaAsync(int citaId);
        Task<List<VeterinarioCalendarioViewModel>> ObtenerVeterinariosConCitasAsync(DateTime fechaInicio, DateTime fechaFin);
    }

    public class RepositorioCalendario : IRepositorioCalendario
    {
        private readonly string connectionString;
        
        // Paleta de colores para veterinarios (fondo, texto)
        private readonly List<(string Fondo, string Texto)> _paletaColores = new()
        {
            ("bg-blue-100", "text-blue-800"),
            ("bg-purple-100", "text-purple-800"),
            ("bg-green-100", "text-green-800"),
            ("bg-amber-100", "text-amber-800"),
            ("bg-pink-100", "text-pink-800"),
            ("bg-cyan-100", "text-cyan-800"),
            ("bg-orange-100", "text-orange-800"),
            ("bg-indigo-100", "text-indigo-800"),
            ("bg-teal-100", "text-teal-800"),
            ("bg-rose-100", "text-rose-800")
        };

        public RepositorioCalendario(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<CalendarioSemanalViewModel> ObtenerCalendarioSemanalAsync(DateTime fecha)
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
                    INNER JOIN Usuarios du ON m.dueñoID = du.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.fechaCita >= @fechaInicio 
                        AND c.fechaCita < @fechaFin
                        AND u.tipoUsuario = 2
                    ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", inicioSemana);
                command.Parameters.AddWithValue("@fechaFin", inicioSemana.AddDays(7));

                var citas = new List<CitaCalendarioViewModel>();
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var veterinarioId = Convert.ToInt32(reader["veterinarioID"]);
                    var colores = ObtenerColorVeterinario(veterinarioId);
                    
                    citas.Add(new CitaCalendarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = veterinarioId,
                        NombreVeterinario = reader["nombreVeterinario"].ToString(),
                        NombreMascota = reader["nombreMascota"].ToString(),
                        NombreDueno = reader["nombreDueno"].ToString(),
                        TipoCita = reader["tipoCita"].ToString(),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteTotal = Convert.ToDecimal(reader["importeCita"]) + 
                                      Convert.ToDecimal(reader["importeAdicional"]),
                        ImporteAdicional = Convert.ToDecimal(reader["importeAdicional"]),
                        Observaciones = reader["observaciones"]?.ToString() ?? "",
                        ColorFondo = colores.Fondo,
                        ColorTexto = colores.Texto
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

        public async Task<CitaCalendarioViewModel> ObtenerDetalleCitaAsync(int citaId)
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
                    INNER JOIN Usuarios du ON m.dueñoID = du.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.id = @citaId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@citaId", citaId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var veterinarioId = Convert.ToInt32(reader["veterinarioID"]);
                    var colores = ObtenerColorVeterinario(veterinarioId);
                    
                    return new CitaCalendarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = veterinarioId,
                        NombreVeterinario = reader["nombreVeterinario"].ToString(),
                        NombreMascota = reader["nombreMascota"].ToString(),
                        NombreDueno = reader["nombreDueno"].ToString(),
                        TipoCita = reader["tipoCita"].ToString(),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteTotal = Convert.ToDecimal(reader["importeCita"]) + 
                                      Convert.ToDecimal(reader["importeAdicional"]),
                        ImporteAdicional = Convert.ToDecimal(reader["importeAdicional"]),
                        Observaciones = reader["observaciones"]?.ToString() ?? "",
                        ColorFondo = colores.Fondo,
                        ColorTexto = colores.Texto
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

        public async Task<List<VeterinarioCalendarioViewModel>> ObtenerVeterinariosConCitasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var veterinarios = new List<VeterinarioCalendarioViewModel>();
            
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        u.id,
                        u.nombre,
                        COUNT(c.id) AS totalCitas
                    FROM Usuarios u
                    LEFT JOIN Citas c ON u.id = c.veterinarioID 
                        AND c.fechaCita >= @fechaInicio 
                        AND c.fechaCita < @fechaFin
                    WHERE u.tipoUsuario = 2
                    GROUP BY u.id, u.nombre
                    ORDER BY u.nombre";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var veterinarioId = Convert.ToInt32(reader["id"]);
                    var colores = ObtenerColorVeterinario(veterinarioId);
                    
                    veterinarios.Add(new VeterinarioCalendarioViewModel
                    {
                        Id = veterinarioId,
                        Nombre = reader["nombre"].ToString(),
                        ColorFondo = colores.Fondo,
                        ColorTexto = colores.Texto,
                        TotalCitasSemana = Convert.ToInt32(reader["totalCitas"])
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener veterinarios: {ex.Message}");
            }

            return veterinarios;
        }

        // Método privado para asignar colores consistentes por veterinario
        private (string Fondo, string Texto) ObtenerColorVeterinario(int veterinarioId)
        {
            var indiceColor = (veterinarioId - 1) % _paletaColores.Count;
            return _paletaColores[indiceColor];
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