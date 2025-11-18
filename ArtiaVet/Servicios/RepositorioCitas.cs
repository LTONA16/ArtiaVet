using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ArtiaVet.Models;
using System.Globalization;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioCitas
    {
        Task<CitasProximasViewModel> ObtenerCitasProximos7DiasAsync();
        Task<CitaViewModel> ObtenerDetalleCitaAsync(int id);
        Task<int> CrearCitaAsync(CitaViewModel cita);
        Task<int> CrearMascotaAsync(MascotaViewModel mascota);
        Task<int> CrearDuenoAsync(DuenoViewModel dueno);
        Task<List<SelectListItem>> ObtenerVeterinariosAsync();
        Task<List<SelectListItem>> ObtenerMascotasAsync();
        Task<List<SelectListItem>> ObtenerTiposCitaAsync();
        Task<List<SelectListItem>> ObtenerTiposAnimalesAsync();
        Task<List<SelectListItem>> ObtenerDuenosAsync();
        Task<List<SelectListItem>> ObtenerAlergiasAsync();
    }

    public class RepositorioCitas : IRepositorioCitas
    {
        private readonly string connectionString;

        public RepositorioCitas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<CitasProximasViewModel> ObtenerCitasProximos7DiasAsync()
        {
            var viewModel = new CitasProximasViewModel();
            var cultura = new CultureInfo("es-MX");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var fechaInicio = DateTime.Today;
                var fechaFin = DateTime.Today.AddDays(7);

                var query = @"
                    SELECT 
                        c.id,
                        c.veterinarioID,
                        c.mascotaID,
                        c.tipoCitaID,
                        c.fechaCita,
                        c.importeAdicional,
                        c.observaciones,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita,
                        tc.importe
                    FROM Citas c
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.fechaCita >= @fechaInicio AND c.fechaCita < @fechaFin
                    ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);

                using var reader = await command.ExecuteReaderAsync();

                var citasPorFecha = new Dictionary<DateTime, List<CitaViewModel>>();

                while (await reader.ReadAsync())
                {
                    var fechaCita = Convert.ToDateTime(reader["fechaCita"]).Date;
                    var importeBase = reader["importe"] != DBNull.Value ? Convert.ToDecimal(reader["importe"]) : 0;
                    var importeAdicional = reader["importeAdicional"] != DBNull.Value ? Convert.ToDecimal(reader["importeAdicional"]) : 0;

                    var cita = new CitaViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = Convert.ToInt32(reader["veterinarioID"]),
                        MascotaID = Convert.ToInt32(reader["mascotaID"]),
                        TipoCitaID = Convert.ToInt32(reader["tipoCitaID"]),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteAdicional = importeAdicional,
                        Observaciones = reader["observaciones"]?.ToString(),
                        NombreVeterinario = reader["nombreVeterinario"]?.ToString(),
                        NombreMascota = reader["nombreMascota"]?.ToString(),
                        NombreDueno = reader["nombreDueno"]?.ToString(),
                        TipoCita = reader["tipoCita"]?.ToString(),
                        HoraInicio = Convert.ToDateTime(reader["fechaCita"]).ToString("h:mm tt", cultura),
                        ImporteTotal = importeBase + importeAdicional
                    };

                    if (!citasPorFecha.ContainsKey(fechaCita))
                    {
                        citasPorFecha[fechaCita] = new List<CitaViewModel>();
                    }
                    citasPorFecha[fechaCita].Add(cita);
                }

                // Crear los objetos CitasPorDia para los próximos 7 días
                for (int i = 0; i < 7; i++)
                {
                    var fecha = fechaInicio.AddDays(i);
                    var diaSemana = cultura.DateTimeFormat.GetDayName(fecha.DayOfWeek);
                    diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);

                    var citasPorDia = new CitasPorDia
                    {
                        Fecha = fecha,
                        DiaSemana = diaSemana,
                        FechaFormateada = fecha.ToString("dd MMMM", cultura),
                        EsHoy = fecha.Date == DateTime.Today,
                        Citas = citasPorFecha.ContainsKey(fecha) ? citasPorFecha[fecha] : new List<CitaViewModel>()
                    };

                    viewModel.CitasPorDias.Add(citasPorDia);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener citas próximas: {ex.Message}");
            }

            return viewModel;
        }

        public async Task<CitaViewModel> ObtenerDetalleCitaAsync(int id)
        {
            CitaViewModel? cita = null;

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        c.id,
                        c.veterinarioID,
                        c.mascotaID,
                        c.tipoCitaID,
                        c.fechaCita,
                        c.importeAdicional,
                        c.observaciones,
                        u.nombre AS nombreVeterinario,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno,
                        tc.nombre AS tipoCita,
                        tc.importe
                    FROM Citas c
                    INNER JOIN Usuarios u ON c.veterinarioID = u.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    INNER JOIN TiposCitas tc ON c.tipoCitaID = tc.id
                    WHERE c.id = @id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var cultura = new CultureInfo("es-MX");
                    var importeBase = reader["importe"] != DBNull.Value ? Convert.ToDecimal(reader["importe"]) : 0;
                    var importeAdicional = reader["importeAdicional"] != DBNull.Value ? Convert.ToDecimal(reader["importeAdicional"]) : 0;

                    cita = new CitaViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        VeterinarioID = Convert.ToInt32(reader["veterinarioID"]),
                        MascotaID = Convert.ToInt32(reader["mascotaID"]),
                        TipoCitaID = Convert.ToInt32(reader["tipoCitaID"]),
                        FechaCita = Convert.ToDateTime(reader["fechaCita"]),
                        ImporteAdicional = importeAdicional,
                        Observaciones = reader["observaciones"]?.ToString(),
                        NombreVeterinario = reader["nombreVeterinario"]?.ToString(),
                        NombreMascota = reader["nombreMascota"]?.ToString(),
                        NombreDueno = reader["nombreDueno"]?.ToString(),
                        TipoCita = reader["tipoCita"]?.ToString(),
                        HoraInicio = Convert.ToDateTime(reader["fechaCita"]).ToString("h:mm tt", cultura),
                        ImporteTotal = importeBase + importeAdicional
                    };
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener detalle de cita: {ex.Message}");
            }

            return cita;
        }

        public async Task<int> CrearCitaAsync(CitaViewModel cita)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Citas (veterinarioID, mascotaID, tipoCitaID, fechaCita, importeAdicional, observaciones)
                    VALUES (@veterinarioID, @mascotaID, @tipoCitaID, @fechaCita, @importeAdicional, @observaciones);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@veterinarioID", cita.VeterinarioID);
                command.Parameters.AddWithValue("@mascotaID", cita.MascotaID);
                command.Parameters.AddWithValue("@tipoCitaID", cita.TipoCitaID);
                command.Parameters.AddWithValue("@fechaCita", cita.FechaCita);
                command.Parameters.AddWithValue("@importeAdicional", cita.ImporteAdicional);
                command.Parameters.AddWithValue("@observaciones", (object?)cita.Observaciones ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine("Cita creada exitosamente");
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al crear cita: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CrearMascotaAsync(MascotaViewModel mascota)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var transaction = connection.BeginTransaction();

                try
                {
                    // Insertar mascota
                    var query = @"
                        INSERT INTO Mascotas (tipoAnimal, dueñoID, raza, edad, nombre, notas)
                        VALUES (@tipoAnimal, @dueñoID, @raza, @edad, @nombre, @notas);
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    using var command = new SqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("@tipoAnimal", mascota.TipoAnimalID);
                    command.Parameters.AddWithValue("@dueñoID", mascota.DueñoID);
                    command.Parameters.AddWithValue("@raza", mascota.Raza);
                    command.Parameters.AddWithValue("@edad", mascota.Edad);
                    command.Parameters.AddWithValue("@nombre", mascota.Nombre);
                    command.Parameters.AddWithValue("@notas", (object?)mascota.Notas ?? DBNull.Value);

                    var mascotaId = Convert.ToInt32(await command.ExecuteScalarAsync());

                    // Insertar alergias si existen
                    if (mascota.AlergiasIDs != null && mascota.AlergiasIDs.Any())
                    {
                        foreach (var alergiaId in mascota.AlergiasIDs)
                        {
                            var queryAlergia = @"
                                INSERT INTO AlergiasMascotas (alergiaID, MascotaID)
                                VALUES (@alergiaID, @mascotaID)";

                            using var cmdAlergia = new SqlCommand(queryAlergia, connection, transaction);
                            cmdAlergia.Parameters.AddWithValue("@alergiaID", alergiaId);
                            cmdAlergia.Parameters.AddWithValue("@mascotaID", mascotaId);
                            await cmdAlergia.ExecuteNonQueryAsync();
                        }
                    }

                    transaction.Commit();
                    Console.WriteLine("Mascota creada exitosamente");
                    return mascotaId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al crear mascota: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CrearDuenoAsync(DuenoViewModel dueno)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO Dueños (nombre, email, numeroTelefono)
                    VALUES (@nombre, @email, @numeroTelefono);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@nombre", dueno.Nombre);
                command.Parameters.AddWithValue("@email", dueno.Email);
                command.Parameters.AddWithValue("@numeroTelefono", dueno.NumeroTelefono);

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine("Dueño creado exitosamente");
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al crear dueño: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SelectListItem>> ObtenerVeterinariosAsync()
        {
            var veterinarios = new List<SelectListItem>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT id, nombre 
                    FROM Usuarios 
                    WHERE tipoUsuario = '2'
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

                var query = @"
                    SELECT m.id, m.nombre, d.nombre AS nombreDueno
                    FROM Mascotas m
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    ORDER BY m.nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    mascotas.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = $"{reader["nombre"]} - Dueño: {reader["nombreDueno"]}"
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

                var query = @"
                    SELECT id, nombre, importe
                    FROM TiposCitas
                    ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var importeBase = Convert.ToDecimal(reader["importe"]);
                    tiposCita.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = $"{reader["nombre"]} - ${importeBase:N2}"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener tipos de cita: {ex.Message}");
            }

            return tiposCita;
        }

        public async Task<List<SelectListItem>> ObtenerTiposAnimalesAsync()
        {
            var tiposAnimales = new List<SelectListItem>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT id, tipo FROM TiposAnimales ORDER BY tipo";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    tiposAnimales.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["tipo"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener tipos de animales: {ex.Message}");
            }

            return tiposAnimales;
        }

        public async Task<List<SelectListItem>> ObtenerDuenosAsync()
        {
            var duenos = new List<SelectListItem>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT id, nombre, email
                    FROM Dueños
                    ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    duenos.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = $"{reader["nombre"]} - {reader["email"]}"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener dueños: {ex.Message}");
            }

            return duenos;
        }

        public async Task<List<SelectListItem>> ObtenerAlergiasAsync()
        {
            var alergias = new List<SelectListItem>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT id, nombre FROM Alergias ORDER BY nombre";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    alergias.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = reader["nombre"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener alergias: {ex.Message}");
            }

            return alergias;
        }
    }
}