using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using ArtiaVet.Models;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioDropdowns
    {
        Task<List<CitaCalendarioViewModel>> ObtenerCitasPorMesAsync(int año, int mes);
        Task<CitaDetalleViewModel> ObtenerDetalleCitaAsync(int citaId);
        Task<List<SelectListItem>> ObtenerVeterinariosAsync();
        Task<List<SelectListItem>> ObtenerMascotasAsync();
        Task<List<SelectListItem>> ObtenerTiposCitaAsync();
        Task<int> CrearCitaAsync(CitaViewModel cita);
    }

    public class RepositorioDropdowns : IRepositorioDropdowns
    {
        private readonly string connectionString;

        public RepositorioDropdowns(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<SelectListItem>> ObtenerVeterinariosAsync()
        {
            var veterinarios = new List<SelectListItem>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT id, nombre 
                             FROM Usuarios 
                             WHERE tipoUsuario = 2
                             ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    veterinarios.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener veterinarios: {ex.Message}");
            }

            return veterinarios;
        }

        public async Task<List<SelectListItem>> ObtenerMascotasAsync()
        {
            var mascotas = new List<SelectListItem>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT id, nombre 
                             FROM Mascotas 
                             ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    mascotas.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener mascotas: {ex.Message}");
            }

            return mascotas;
        }

        public async Task<List<SelectListItem>> ObtenerTiposCitaAsync()
        {
            var tiposCita = new List<SelectListItem>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT id, nombre 
                             FROM TiposCitas 
                             ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tiposCita.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener tipos de cita: {ex.Message}");
            }

            return tiposCita;
        }

        public async Task<int> CrearCitaAsync(CitaViewModel cita)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("CrearCitaYActualizarInventario", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Agregar parámetros
                command.Parameters.AddWithValue("@p_veterinarioID", cita.VeterinarioID);
                command.Parameters.AddWithValue("@p_mascotaID", cita.MascotaID);
                command.Parameters.AddWithValue("@p_tipoCitaID", cita.TipoCitaID);
                command.Parameters.AddWithValue("@p_fechaCita", cita.FechaCita);
                command.Parameters.AddWithValue("@p_importeAdicional", cita.ImporteAdicional);
                command.Parameters.AddWithValue("@p_observaciones",
                    string.IsNullOrEmpty(cita.Observaciones) ? (object)DBNull.Value : cita.Observaciones);

                // Ejecutar el stored procedure
                await command.ExecuteNonQueryAsync();

                Console.WriteLine("Cita creada exitosamente");
                return 1; // Éxito
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error SQL al crear cita: {ex.Message}");
                Console.WriteLine($"Número de error: {ex.Number}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear cita: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CitaCalendarioViewModel>> ObtenerCitasPorMesAsync(int año, int mes)
        {
            var citas = new List<CitaCalendarioViewModel>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
            SELECT 
                c.id,
                c.fechaCita,
                c.importeAdicional,
                c.observaciones,
                u.nombre as NombreVeterinario,
                m.nombre as NombreMascota,
                tc.nombre as TipoCita
            FROM Citas c
            INNER JOIN Usuarios u ON c.veterinarioID = u.id
            INNER JOIN Mascotas m ON c.mascotaID = m.id
            INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
            WHERE YEAR(c.fechaCita) = @año AND MONTH(c.fechaCita) = @mes
            ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@año", año);
                command.Parameters.AddWithValue("@mes", mes);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    citas.Add(new CitaCalendarioViewModel
                    {
                        Id = (int)reader["id"],
                        FechaCita = (DateTime)reader["fechaCita"],
                        NombreVeterinario = reader["NombreVeterinario"].ToString(),
                        NombreMascota = reader["NombreMascota"].ToString(),
                        TipoCita = reader["TipoCita"].ToString(),
                        ImporteAdicional = (decimal)reader["importeAdicional"],
                        Observaciones = reader["observaciones"]?.ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener citas: {ex.Message}");
            }

            return citas;
        }

        public async Task<CitaDetalleViewModel> ObtenerDetalleCitaAsync(int citaId)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
            SELECT 
                c.id,
                c.fechaCita,
                c.importeAdicional,
                c.observaciones,
                u.nombre as NombreVeterinario,
                m.nombre as NombreMascota,
                tc.nombre as TipoCita,
                ISNULL(due.nombre, '') as NombreDueño,
                ISNULL(due.telefono, '') as TelefonoDueño
            FROM Citas c
            INNER JOIN Usuarios u ON c.veterinarioID = u.id
            INNER JOIN Mascotas m ON c.mascotaID = m.id
            INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
            LEFT JOIN Usuarios due ON m.dueñoID = due.id
            WHERE c.id = @citaId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@citaId", citaId);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new CitaDetalleViewModel
                    {
                        Id = (int)reader["id"],
                        FechaCita = (DateTime)reader["fechaCita"],
                        NombreVeterinario = reader["NombreVeterinario"].ToString(),
                        NombreMascota = reader["NombreMascota"].ToString(),
                        TipoCita = reader["TipoCita"].ToString(),
                        ImporteAdicional = (decimal)reader["importeAdicional"],
                        Observaciones = reader["observaciones"]?.ToString(),
                        NombreDueño = reader["NombreDueño"].ToString(),
                        TelefonoDueño = reader["TelefonoDueño"].ToString()
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener detalle de cita: {ex.Message}");
            }

            return null;
        }
    }
}