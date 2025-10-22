using Microsoft.Data.SqlClient;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioTest
    {
        Task<bool> TestConnectionAsync();
        bool TestConnection();
    }

    public class RepositorioTest : IRepositorioTest
    {
        private readonly string connectionString;

        public RepositorioTest(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método asíncrono (recomendado)
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT 1", connection);
                var result = await command.ExecuteScalarAsync();

                Console.WriteLine($"Conexión con la base de datos exitosa. Resultado: {result}");
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error de SQL Server: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }

        // Método síncrono (alternativa)
        public bool TestConnection()
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Open();

                using var command = new SqlCommand("SELECT 1", connection);
                var result = command.ExecuteScalar();

                Console.WriteLine($"Conexión exitosa. Resultado: {result}");
                return true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error de SQL Server: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
                return false;
            }
        }
    }
}