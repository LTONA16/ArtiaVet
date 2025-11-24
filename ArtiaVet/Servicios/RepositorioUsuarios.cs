using ArtiaVet.Models;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioUsuarios
    {
        Task<UsuarioViewModel?> ValidarCredencialesAsync(string username, string password);
        Task<UsuarioViewModel?> ObtenerUsuarioPorIdAsync(int id);
    }

    public class RepositorioUsuarios : IRepositorioUsuarios
    {
        private readonly string connectionString;

        public RepositorioUsuarios(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"[REPO CONSTRUCTOR] Connection String: {connectionString}");
        }

        // Método helper para generar hash SHA256 en C#
        private byte[] GenerarHashSHA256(string texto)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // IMPORTANTE: Usar la misma codificación que SQL Server (UTF-8)
                byte[] bytes = Encoding.UTF8.GetBytes(texto);
                return sha256.ComputeHash(bytes);
            }
        }

        public async Task<UsuarioViewModel?> ValidarCredencialesAsync(string username, string password)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("[REPO] ===== INICIO VALIDACIÓN =====");
            Console.WriteLine($"[REPO] Username recibido: '{username}'");
            Console.WriteLine($"[REPO] Password recibido (length): {password?.Length ?? 0} caracteres");
            
            try
            {
                Console.WriteLine("[REPO] Abriendo conexión a BD...");
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                Console.WriteLine("[REPO] ✓ Conexión abierta exitosamente");

                // Generar hash del password en C#
                byte[] passwordHash = GenerarHashSHA256(password);
                string passwordHashHex = BitConverter.ToString(passwordHash).Replace("-", "");
                Console.WriteLine($"[REPO] Hash generado en C# (HEX): {passwordHashHex}");

                // Query simplificado - comparar directamente los bytes
                var query = @"SELECT 
                                u.id,
                                u.username,
                                u.nombre,
                                u.tipoUsuario,
                                u.imagen,
                                tu.tipo AS tipoUsuarioNombre
                             FROM Usuarios u
                             INNER JOIN TipoUsuario tu ON u.tipoUsuario = tu.id
                             WHERE u.username = @username 
                             AND u.password = @passwordHash";

                Console.WriteLine($"[REPO] Query: {query.Replace("\n", " ").Replace("  ", " ").Trim()}");

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                // Pasar el hash como VARBINARY
                command.Parameters.Add("@passwordHash", System.Data.SqlDbType.VarBinary).Value = passwordHash;
                
                Console.WriteLine($"[REPO] Parámetro @username = '{username}'");
                Console.WriteLine($"[REPO] Parámetro @passwordHash = byte[{passwordHash.Length}]");

                Console.WriteLine("[REPO] Ejecutando query...");
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    Console.WriteLine("[REPO] ✓✓✓ ÉXITO: Usuario encontrado y validado");
                    var usuario = new UsuarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Nombre = reader["nombre"].ToString(),
                        TipoUsuario = Convert.ToInt32(reader["tipoUsuario"]),
                        TipoUsuarioNombre = reader["tipoUsuarioNombre"].ToString(),
                        Imagen = reader["imagen"] != DBNull.Value 
                            ? (byte[])reader["imagen"] 
                            : null
                    };
                    Console.WriteLine($"[REPO] Usuario.Id: {usuario.Id}");
                    Console.WriteLine($"[REPO] Usuario.Nombre: {usuario.Nombre}");
                    Console.WriteLine($"[REPO] Usuario.TipoUsuario: {usuario.TipoUsuario}");
                    Console.WriteLine($"[REPO] Usuario.TipoUsuarioNombre: {usuario.TipoUsuarioNombre}");
                    Console.WriteLine("[REPO] ===== FIN VALIDACIÓN (ÉXITO) =====");
                    Console.WriteLine("========================================\n");
                    return usuario;
                }

                Console.WriteLine("[REPO] ✗✗✗ FALLO: Query no devolvió resultados");
                Console.WriteLine("[REPO] ===== FIN VALIDACIÓN (FALLO) =====");
                Console.WriteLine("========================================\n");
                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[REPO] ✗✗✗ ERROR SQL: {ex.Message}");
                Console.WriteLine($"[REPO] Error Number: {ex.Number}");
                Console.WriteLine($"[REPO] Error State: {ex.State}");
                Console.WriteLine($"[REPO] StackTrace: {ex.StackTrace}");
                Console.WriteLine("[REPO] ===== FIN VALIDACIÓN (ERROR) =====");
                Console.WriteLine("========================================\n");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[REPO] ✗✗✗ ERROR GENERAL: {ex.Message}");
                Console.WriteLine($"[REPO] Tipo: {ex.GetType().Name}");
                Console.WriteLine($"[REPO] StackTrace: {ex.StackTrace}");
                Console.WriteLine("[REPO] ===== FIN VALIDACIÓN (ERROR) =====");
                Console.WriteLine("========================================\n");
                throw;
            }
        }

        public async Task<UsuarioViewModel?> ObtenerUsuarioPorIdAsync(int id)
        {
            Console.WriteLine($"[REPO] ObtenerUsuarioPorIdAsync - ID: {id}");
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT 
                                u.id,
                                u.username,
                                u.nombre,
                                u.tipoUsuario,
                                u.imagen,
                                tu.tipo AS tipoUsuarioNombre
                             FROM Usuarios u
                             INNER JOIN TipoUsuario tu ON u.tipoUsuario = tu.id
                             WHERE u.id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new UsuarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        Username = reader["username"].ToString(),
                        Nombre = reader["nombre"].ToString(),
                        TipoUsuario = Convert.ToInt32(reader["tipoUsuario"]),
                        TipoUsuarioNombre = reader["tipoUsuarioNombre"].ToString(),
                        Imagen = reader["imagen"] != DBNull.Value 
                            ? (byte[])reader["imagen"] 
                            : null
                    };
                }

                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"[REPO] Error al obtener usuario: {ex.Message}");
                throw;
            }
        }
    }
}