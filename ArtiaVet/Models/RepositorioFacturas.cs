using Microsoft.Data.SqlClient;
using ArtiaVet.Models;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioFacturas
    {
        Task<List<FacturaViewModel>> ObtenerFacturasAsync();
        Task<List<FacturaViewModel>> BuscarFacturasAsync(string? terminoBusqueda, DateTime? fechaInicio, DateTime? fechaFin);
        Task<FacturaViewModel?> ObtenerFacturaPorIdAsync(int id);
        Task<List<CitaParaFacturaViewModel>> ObtenerCitasSinFacturarAsync();
        Task<int> CrearFacturaAsync(FacturaViewModel factura);
        Task ActualizarFacturaAsync(FacturaViewModel factura);
        Task EliminarFacturaAsync(int id);
    }

    public class RepositorioFacturas : IRepositorioFacturas
    {
        private readonly string connectionString;

        public RepositorioFacturas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<FacturaViewModel>> ObtenerFacturasAsync()
        {
            var facturas = new List<FacturaViewModel>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        f.id,
                        f.citaID,
                        f.monto,
                        f.iva,
                        f.total,
                        c.fechaCita,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita
                    FROM Facturas f
                    INNER JOIN Citas c ON f.citaID = c.id
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    ORDER BY c.fechaCita DESC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    facturas.Add(MapearFacturaDesdeReader(reader));
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener facturas: {ex.Message}");
                throw;
            }

            return facturas;
        }

        public async Task<List<FacturaViewModel>> BuscarFacturasAsync(string? terminoBusqueda, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var facturas = new List<FacturaViewModel>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        f.id,
                        f.citaID,
                        f.monto,
                        f.iva,
                        f.total,
                        c.fechaCita,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita
                    FROM Facturas f
                    INNER JOIN Citas c ON f.citaID = c.id
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE 1=1";

                if (!string.IsNullOrWhiteSpace(terminoBusqueda))
                {
                    query += @" AND (
                        d.nombre LIKE @termino OR 
                        m.nombre LIKE @termino OR 
                        u.nombre LIKE @termino OR
                        tc.nombre LIKE @termino
                    )";
                }

                if (fechaInicio.HasValue)
                {
                    query += " AND c.fechaCita >= @fechaInicio";
                }

                if (fechaFin.HasValue)
                {
                    query += " AND c.fechaCita <= @fechaFin";
                }

                query += " ORDER BY c.fechaCita DESC";

                using var command = new SqlCommand(query, connection);

                if (!string.IsNullOrWhiteSpace(terminoBusqueda))
                {
                    command.Parameters.AddWithValue("@termino", $"%{terminoBusqueda}%");
                }

                if (fechaInicio.HasValue)
                {
                    command.Parameters.AddWithValue("@fechaInicio", fechaInicio.Value.Date);
                }

                if (fechaFin.HasValue)
                {
                    command.Parameters.AddWithValue("@fechaFin", fechaFin.Value.Date.AddDays(1).AddSeconds(-1));
                }

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    facturas.Add(MapearFacturaDesdeReader(reader));
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al buscar facturas: {ex.Message}");
                throw;
            }

            return facturas;
        }

        public async Task<FacturaViewModel?> ObtenerFacturaPorIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        f.id,
                        f.citaID,
                        f.monto,
                        f.iva,
                        f.total,
                        c.fechaCita,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita
                    FROM Facturas f
                    INNER JOIN Citas c ON f.citaID = c.id
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE f.id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapearFacturaDesdeReader(reader);
                }

                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener factura por ID: {ex.Message}");
                throw;
            }
        }

        public async Task<List<CitaParaFacturaViewModel>> ObtenerCitasSinFacturarAsync()
        {
            var citas = new List<CitaParaFacturaViewModel>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        c.id,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita,
                        u.nombre AS nombreVeterinario,
                        c.fechaCita,
                        tc.importe AS importeBase,
                        c.importeAdicional,
                        (tc.importe + c.importeAdicional) AS importeTotal
                    FROM Citas c
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Facturas f WHERE f.citaID = c.id
                    )
                    AND c.fechaCita <= GETDATE()
                    ORDER BY c.fechaCita DESC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    citas.Add(new CitaParaFacturaViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        NombreMascota = reader["nombreMascota"].ToString(),
                        NombreDueno = reader["nombreDueno"].ToString(),
                        TipoCita = reader["tipoCita"].ToString(),
                        NombreVeterinario = reader["nombreVeterinario"].ToString(),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteBase = Convert.ToDecimal(reader["importeBase"]),
                        ImporteAdicional = Convert.ToDecimal(reader["importeAdicional"]),
                        ImporteTotal = Convert.ToDecimal(reader["importeTotal"])
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener citas sin facturar: {ex.Message}");
                throw;
            }

            return citas;
        }

        public async Task<int> CrearFacturaAsync(FacturaViewModel factura)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Verificar si la cita ya tiene factura
                var queryVerificar = "SELECT COUNT(*) FROM Facturas WHERE citaID = @citaID";
                using var commandVerificar = new SqlCommand(queryVerificar, connection);
                commandVerificar.Parameters.AddWithValue("@citaID", factura.CitaID);
                
                var count = (int)await commandVerificar.ExecuteScalarAsync();
                if (count > 0)
                {
                    throw new InvalidOperationException("Esta cita ya tiene una factura registrada");
                }

                var query = @"
                    INSERT INTO Facturas (citaID, monto, iva, total)
                    VALUES (@citaID, @monto, @iva, @total);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@citaID", factura.CitaID);
                command.Parameters.AddWithValue("@monto", factura.Monto);
                command.Parameters.AddWithValue("@iva", factura.Iva);
                command.Parameters.AddWithValue("@total", factura.Total);

                var id = (int)await command.ExecuteScalarAsync();
                return id;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al crear factura: {ex.Message}");
                throw;
            }
        }

        public async Task ActualizarFacturaAsync(FacturaViewModel factura)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE Facturas 
                    SET citaID = @citaID,
                        monto = @monto,
                        iva = @iva,
                        total = @total
                    WHERE id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", factura.Id);
                command.Parameters.AddWithValue("@citaID", factura.CitaID);
                command.Parameters.AddWithValue("@monto", factura.Monto);
                command.Parameters.AddWithValue("@iva", factura.Iva);
                command.Parameters.AddWithValue("@total", factura.Total);

                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al actualizar factura: {ex.Message}");
                throw;
            }
        }

        public async Task EliminarFacturaAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "DELETE FROM Facturas WHERE id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al eliminar factura: {ex.Message}");
                throw;
            }
        }

        private FacturaViewModel MapearFacturaDesdeReader(SqlDataReader reader)
        {
            var fechaCita = Convert.ToDateTime(reader["fechaCita"]);
            
            return new FacturaViewModel
            {
                Id = Convert.ToInt32(reader["id"]),
                CitaID = Convert.ToInt32(reader["citaID"]),
                Monto = Convert.ToDecimal(reader["monto"]),
                Iva = Convert.ToDecimal(reader["iva"]),
                Total = Convert.ToDecimal(reader["total"]),
                FechaCita = fechaCita,
                NombreVeterinario = reader["nombreVeterinario"].ToString(),
                NombreMascota = reader["nombreMascota"].ToString(),
                NombreDueno = reader["nombreDueno"].ToString(),
                TipoCita = reader["tipoCita"].ToString(),
                FechaFormateada = fechaCita.ToString("dd/MM/yyyy")
            };
        }
    }
}