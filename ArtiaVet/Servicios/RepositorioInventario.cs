using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using ArtiaVet.Models;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioInventario
    {
        Task<List<InventarioViewModel>> ObtenerTop5StockBajoAsync();
        Task<List<InventarioViewModel>> ObtenerInventarioAsync();
        Task<List<SelectListItem>> ObtenerInsumosAsync();
        Task<int> CrearInventarioAsync(InventarioViewModel inventario);
        Task<int> ActualizarInventarioAsync(InventarioViewModel inventario);
        Task<int> EliminarInventarioAsync(int id);
        Task<List<InventarioViewModel>> ObtenerStockBajoAsync();
    }

    public class RepositorioInventario : IRepositorioInventario
    {
        private readonly string connectionString;

        public RepositorioInventario(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<InventarioViewModel>> ObtenerInventarioAsync()
        {
            var inventario = new List<InventarioViewModel>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT 
                                i.id,
                                i.insumoID,
                                ins.nombre AS nombreInsumo,
                                i.cantidad,
                                ins.precioUnitario
                             FROM Inventario i
                             INNER JOIN Insumos ins ON i.insumoID = ins.id
                             ORDER BY ins.nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    inventario.Add(new InventarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        InsumoID = Convert.ToInt32(reader["insumoID"]),
                        NombreInsumo = reader["nombreInsumo"].ToString(),
                        Cantidad = Convert.ToInt32(reader["cantidad"]),
                        PrecioUnitario = Convert.ToDecimal(reader["precioUnitario"])
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener inventario: {ex.Message}");
            }

            return inventario;
        }

        public async Task<List<SelectListItem>> ObtenerInsumosAsync()
        {
            var insumos = new List<SelectListItem>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT id, nombre, precioUnitario 
                             FROM Insumos 
                             ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var precio = Convert.ToDecimal(reader["precioUnitario"]);
                    insumos.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = $"{reader["nombre"]} - ${precio:N2}"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener insumos: {ex.Message}");
            }

            return insumos;
        }

        public async Task<int> CrearInventarioAsync(InventarioViewModel inventario)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // PASO 1: Verificar si ya existe este insumo en el inventario
                var queryVerificar = "SELECT COUNT(1) FROM Inventario WHERE insumoID = @insumoID";
                using var commandVerificar = new SqlCommand(queryVerificar, connection);
                commandVerificar.Parameters.AddWithValue("@insumoID", inventario.InsumoID);

                int existe = (int)await commandVerificar.ExecuteScalarAsync();

                if (existe > 0)
                {
                    // Si ya existe, lanzamos un error personalizado
                    throw new InvalidOperationException("Este insumo ya está registrado en el inventario. Busca el item en la lista y edítalo para cambiar la cantidad.");
                }

                // PASO 2: Si no existe, procedemos a Insertar
                var queryInsert = @"INSERT INTO Inventario (insumoID, cantidad) 
                           VALUES (@insumoID, @cantidad);
                           SELECT CAST(SCOPE_IDENTITY() as int)";

                using var commandInsert = new SqlCommand(queryInsert, connection);
                commandInsert.Parameters.AddWithValue("@insumoID", inventario.InsumoID);
                commandInsert.Parameters.AddWithValue("@cantidad", inventario.Cantidad);

                var result = await commandInsert.ExecuteScalarAsync();
                Console.WriteLine("Inventario creado exitosamente");
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error de SQL al crear inventario: {ex.Message}");
                throw; // Relanzamos para que el controller lo maneje
            }
        }

        public async Task<int> ActualizarInventarioAsync(InventarioViewModel inventario)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"UPDATE Inventario 
                             SET insumoID = @insumoID,
                                 cantidad = @cantidad
                             WHERE id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", inventario.Id);
                command.Parameters.AddWithValue("@insumoID", inventario.InsumoID);
                command.Parameters.AddWithValue("@cantidad", inventario.Cantidad);

                var result = await command.ExecuteNonQueryAsync();
                Console.WriteLine("Inventario actualizado exitosamente");
                return result;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al actualizar inventario: {ex.Message}");
                throw;
            }
        }

        public async Task<int> EliminarInventarioAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "DELETE FROM Inventario WHERE id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                var result = await command.ExecuteNonQueryAsync();
                Console.WriteLine("Inventario eliminado exitosamente");
                return result;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al eliminar inventario: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InventarioViewModel>> ObtenerStockBajoAsync()
        {
            var inventario = new List<InventarioViewModel>();
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT TOP 5
                                i.id,
                                i.insumoID,
                                ins.nombre AS nombreInsumo,
                                i.cantidad,
                                ins.precioUnitario
                             FROM Inventario i
                             INNER JOIN Insumos ins ON i.insumoID = ins.id
                             WHERE i.cantidad < 50
                             ORDER BY i.cantidad ASC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    inventario.Add(new InventarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        InsumoID = Convert.ToInt32(reader["insumoID"]),
                        NombreInsumo = reader["nombreInsumo"].ToString(),
                        Cantidad = Convert.ToInt32(reader["cantidad"]),
                        PrecioUnitario = Convert.ToDecimal(reader["precioUnitario"])
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener stock bajo: {ex.Message}");
            }

            return inventario;
        }

        public async Task<List<InventarioViewModel>> ObtenerTop5StockBajoAsync()
        {
            var inventarios = new List<InventarioViewModel>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                            SELECT TOP 5
                                i.id,
                                i.insumoID,
                                i.cantidad,
                                ins.nombre AS nombreInsumo,
                                ins.precioUnitario
                            FROM Inventario i
                            INNER JOIN Insumos ins ON i.insumoID = ins.id
                            ORDER BY i.cantidad ASC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var inventario = new InventarioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        InsumoID = Convert.ToInt32(reader["insumoID"]),
                        NombreInsumo = reader["nombreInsumo"]?.ToString(),
                        Cantidad = Convert.ToInt32(reader["cantidad"]),
                        PrecioUnitario = Convert.ToDecimal(reader["precioUnitario"])
                    };

                    inventarios.Add(inventario);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener top 5 stock bajo: {ex.Message}");
            }

            return inventarios;
        }
    }
}