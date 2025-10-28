using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using ArtiaVet.Models;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioDropdowns
    {
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
    }
}