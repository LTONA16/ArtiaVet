using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ArtiaVet.Models;
using System.Globalization;

namespace ArtiaVet.Servicios
{
    public interface IRepositorioCitasVeterinario
    {
        Task<List<CitaViewModel>> ObtenerCitasDelDiaAsync(int veterinarioId);
        Task<List<RecordatorioViewModel>> ObtenerRecordatoriosHoyAsync(int veterinarioId, int cantidad = 5);
        Task<CitasProximasViewModel> ObtenerCitasProximos7DiasAsync(int veterinarioId);
        Task<CitaViewModel> ObtenerDetalleCitaAsync(int id, int veterinarioId);
        Task<int> CrearCitaAsync(CitaViewModel cita);
        Task ActualizarCitaAsync(CitaViewModel cita, int veterinarioId);
        Task EliminarCitaAsync(int id, int veterinarioId);
        Task<int> CrearMascotaAsync(MascotaViewModel mascota);
        Task<int> CrearDuenoAsync(DuenoViewModel dueno);
        Task<List<SelectListItem>> ObtenerMascotasAsync();
        Task<List<SelectListItem>> ObtenerTiposCitaAsync();
        Task<List<SelectListItem>> ObtenerTiposAnimalesAsync();
        Task<List<SelectListItem>> ObtenerDuenosAsync();
        Task<List<SelectListItem>> ObtenerAlergiasAsync();
        Task<RecordatoriosProximosViewModel> ObtenerRecordatoriosProximos7DiasAsync(int veterinarioId);
        Task<List<SelectListItem>> ObtenerCitasActivasAsync(int veterinarioId);
        Task<int> CrearRecordatorioAsync(RecordatorioViewModel recordatorio, int veterinarioId);
        Task<RecordatorioViewModel> ObtenerRecordatorioPorIdAsync(int id, int veterinarioId);
        Task ActualizarRecordatorioAsync(RecordatorioViewModel recordatorio, int veterinarioId);
        Task EliminarRecordatorioAsync(int id, int veterinarioId);
        Task<RecordatorioViewModel> ObtenerDetalleRecordatorioCompletoAsync(int id, int veterinarioId);
    }

    public class RepositorioCitasVeterinario : IRepositorioCitasVeterinario
    {
        private readonly string connectionString;

        public RepositorioCitasVeterinario(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<CitasProximasViewModel> ObtenerCitasProximos7DiasAsync(int veterinarioId)
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
                    AND c.veterinarioID = @veterinarioId
                    ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

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

        public async Task<CitaViewModel> ObtenerDetalleCitaAsync(int id, int veterinarioId)
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
                    WHERE c.id = @id
                    AND c.veterinarioID = @veterinarioId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

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

        public async Task ActualizarCitaAsync(CitaViewModel cita, int veterinarioId)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    UPDATE Citas 
                    SET mascotaID = @mascotaID,
                        tipoCitaID = @tipoCitaID,
                        fechaCita = @fechaCita,
                        importeAdicional = @importeAdicional,
                        observaciones = @observaciones
                    WHERE id = @id
                    AND veterinarioID = @veterinarioId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", cita.Id);
                command.Parameters.AddWithValue("@mascotaID", cita.MascotaID);
                command.Parameters.AddWithValue("@tipoCitaID", cita.TipoCitaID);
                command.Parameters.AddWithValue("@fechaCita", cita.FechaCita);
                command.Parameters.AddWithValue("@importeAdicional", cita.ImporteAdicional);
                command.Parameters.AddWithValue("@observaciones", (object?)cita.Observaciones ?? DBNull.Value);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Cita actualizada exitosamente");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al actualizar cita: {ex.Message}");
                throw;
            }
        }

        public async Task EliminarCitaAsync(int id, int veterinarioId)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    DELETE FROM Citas 
                    WHERE id = @id 
                    AND veterinarioID = @veterinarioId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                await command.ExecuteNonQueryAsync();
                Console.WriteLine("Cita eliminada exitosamente");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al eliminar cita: {ex.Message}");
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

        public async Task<RecordatoriosProximosViewModel> ObtenerRecordatoriosProximos7DiasAsync(int veterinarioId)
        {
            var viewModel = new RecordatoriosProximosViewModel();
            var cultura = new CultureInfo("es-MX");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var fechaInicio = DateTime.Today;
                var fechaFin = DateTime.Today.AddDays(7);

                var query = @"
                    SELECT 
                        r.id,
                        r.citaID,
                        r.fechaRecordatorio,
                        r.asunto,
                        r.mensaje,
                        d.nombre AS nombreDueno,
                        m.nombre AS nombreMascota,
                        d.numeroTelefono AS telefonoDueno,
                        d.email AS emailDueno
                    FROM RecordatoriosDueños r
                    INNER JOIN Citas c ON r.citaID = c.id
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    WHERE r.fechaRecordatorio >= @fechaInicio AND r.fechaRecordatorio < @fechaFin
                    AND c.veterinarioID = @veterinarioId
                    ORDER BY r.fechaRecordatorio";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                command.Parameters.AddWithValue("@fechaFin", fechaFin);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                using var reader = await command.ExecuteReaderAsync();

                var recordatoriosPorFecha = new Dictionary<DateTime, List<RecordatorioViewModel>>();

                while (await reader.ReadAsync())
                {
                    var fechaRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]).Date;

                    var recordatorio = new RecordatorioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        CitaID = Convert.ToInt32(reader["citaID"]),
                        FechaRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]),
                        Asunto = reader["asunto"]?.ToString(),
                        Mensaje = reader["mensaje"]?.ToString(),
                        NombreDueno = reader["nombreDueno"]?.ToString(),
                        NombreMascota = reader["nombreMascota"]?.ToString(),
                        TelefonoDueno = reader["telefonoDueno"]?.ToString(),
                        EmailDueno = reader["emailDueno"]?.ToString(),
                        HoraRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]).ToString("HH:mm")
                    };

                    if (!recordatoriosPorFecha.ContainsKey(fechaRecordatorio))
                    {
                        recordatoriosPorFecha[fechaRecordatorio] = new List<RecordatorioViewModel>();
                    }
                    recordatoriosPorFecha[fechaRecordatorio].Add(recordatorio);
                }

                // Crear los objetos RecordatoriosPorDia para los próximos 7 días
                for (int i = 0; i < 7; i++)
                {
                    var fecha = fechaInicio.AddDays(i);
                    var diaSemana = cultura.DateTimeFormat.GetDayName(fecha.DayOfWeek);
                    diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);

                    var recordatoriosPorDia = new RecordatoriosPorDia
                    {
                        Fecha = fecha,
                        DiaSemana = diaSemana,
                        FechaFormateada = fecha.ToString("dd MMMM", cultura),
                        EsHoy = fecha.Date == DateTime.Today,
                        Recordatorios = recordatoriosPorFecha.ContainsKey(fecha) ? recordatoriosPorFecha[fecha] : new List<RecordatorioViewModel>()
                    };

                    viewModel.RecordatoriosPorDias.Add(recordatoriosPorDia);
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener recordatorios próximos: {ex.Message}");
            }

            return viewModel;
        }

        public async Task<List<SelectListItem>> ObtenerCitasActivasAsync(int veterinarioId)
        {
            var citas = new List<SelectListItem>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var fechaHoy = DateTime.Today;

                var query = @"
                    SELECT 
                        c.id,
                        c.fechaCita,
                        m.nombre AS nombreMascota,
                        d.nombre AS nombreDueno
                    FROM Citas c
                    INNER JOIN Mascotas m ON c.mascotaID = m.id
                    INNER JOIN Dueños d ON m.dueñoID = d.id
                    WHERE c.fechaCita >= @fechaHoy
                    AND c.veterinarioID = @veterinarioId
                    ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaHoy", fechaHoy);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var fechaCita = Convert.ToDateTime(reader["fechaCita"]);
                    citas.Add(new SelectListItem
                    {
                        Value = reader["id"].ToString(),
                        Text = $"{reader["nombreDueno"]} - {reader["nombreMascota"]} ({fechaCita:dd/MM/yyyy HH:mm})"
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener citas activas: {ex.Message}");
            }

            return citas;
        }

        public async Task<int> CrearRecordatorioAsync(RecordatorioViewModel recordatorio, int veterinarioId)
        {
            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Verificar que la cita pertenezca al veterinario
                var queryVerificar = @"
                    SELECT COUNT(*) 
                    FROM Citas 
                    WHERE id = @citaID 
                    AND veterinarioID = @veterinarioId";

                using var cmdVerificar = new SqlCommand(queryVerificar, connection);
                cmdVerificar.Parameters.AddWithValue("@citaID", recordatorio.CitaID);
                cmdVerificar.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                var count = (int)await cmdVerificar.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new UnauthorizedAccessException("No tiene permiso para crear recordatorios para esta cita");
                }

                var query = @"
                    INSERT INTO RecordatoriosDueños (citaID, fechaRecordatorio, asunto, mensaje)
                    VALUES (@citaID, @fechaRecordatorio, @asunto, @mensaje);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@citaID", recordatorio.CitaID);
                command.Parameters.AddWithValue("@fechaRecordatorio", recordatorio.FechaRecordatorio);
                command.Parameters.AddWithValue("@asunto", recordatorio.Asunto);
                command.Parameters.AddWithValue("@mensaje", recordatorio.Mensaje);

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine("Recordatorio creado exitosamente");
                return Convert.ToInt32(result);
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al crear recordatorio: {ex.Message}");
                throw;
            }
        }

        public async Task<RecordatorioViewModel> ObtenerRecordatorioPorIdAsync(int id, int veterinarioId)
        {
            RecordatorioViewModel? recordatorio = null;

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"SELECT 
                    r.id,
                    r.citaID,
                    r.fechaRecordatorio,
                    r.asunto,
                    r.mensaje
                FROM RecordatoriosDueños r
                INNER JOIN Citas c ON r.citaID = c.id
                WHERE r.id = @id
                AND c.veterinarioID = @veterinarioId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                recordatorio = new RecordatorioViewModel
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CitaID = Convert.ToInt32(reader["citaID"]),
                    FechaRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]),
                    Asunto = reader["asunto"]?.ToString(),
                    Mensaje = reader["mensaje"]?.ToString()
                };
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error al obtener recordatorio por ID: {ex.Message}");
        }

        return recordatorio;
    }

    public async Task ActualizarRecordatorioAsync(RecordatorioViewModel recordatorio, int veterinarioId)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                UPDATE RecordatoriosDueños 
                SET citaID = @citaID,
                    fechaRecordatorio = @fechaRecordatorio,
                    asunto = @asunto,
                    mensaje = @mensaje
                WHERE id = @id
                AND EXISTS (
                    SELECT 1 FROM Citas c 
                    WHERE c.id = RecordatoriosDueños.citaID 
                    AND c.veterinarioID = @veterinarioId
                )";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", recordatorio.Id);
            command.Parameters.AddWithValue("@citaID", recordatorio.CitaID);
            command.Parameters.AddWithValue("@fechaRecordatorio", recordatorio.FechaRecordatorio);
            command.Parameters.AddWithValue("@asunto", recordatorio.Asunto);
            command.Parameters.AddWithValue("@mensaje", recordatorio.Mensaje);
            command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Recordatorio actualizado exitosamente");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error al actualizar recordatorio: {ex.Message}");
            throw;
        }
    }

    public async Task EliminarRecordatorioAsync(int id, int veterinarioId)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                DELETE FROM RecordatoriosDueños 
                WHERE id = @id
                AND EXISTS (
                    SELECT 1 FROM Citas c 
                    WHERE c.id = RecordatoriosDueños.citaID 
                    AND c.veterinarioID = @veterinarioId
                )";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Recordatorio eliminado exitosamente");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error al eliminar recordatorio: {ex.Message}");
            throw;
        }
    }

    public async Task<RecordatorioViewModel> ObtenerDetalleRecordatorioCompletoAsync(int id, int veterinarioId)
    {
        RecordatorioViewModel? recordatorio = null;

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    r.id,
                    r.citaID,
                    r.fechaRecordatorio,
                    r.asunto,
                    r.mensaje,
                    d.nombre AS nombreDueno,
                    m.nombre AS nombreMascota,
                    d.numeroTelefono AS telefonoDueno,
                    d.email AS emailDueno
                FROM RecordatoriosDueños r
                INNER JOIN Citas c ON r.citaID = c.id
                INNER JOIN Mascotas m ON c.mascotaID = m.id
                INNER JOIN Dueños d ON m.dueñoID = d.id
                WHERE r.id = @id
                AND c.veterinarioID = @veterinarioId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                recordatorio = new RecordatorioViewModel
                {
                    Id = Convert.ToInt32(reader["id"]),
                    CitaID = Convert.ToInt32(reader["citaID"]),
                    FechaRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]),
                    Asunto = reader["asunto"]?.ToString(),
                    Mensaje = reader["mensaje"]?.ToString(),
                    NombreDueno = reader["nombreDueno"]?.ToString(),
                    NombreMascota = reader["nombreMascota"]?.ToString(),
                    TelefonoDueno = reader["telefonoDueno"]?.ToString(),
                    EmailDueno = reader["emailDueno"]?.ToString(),
                    HoraRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]).ToString("HH:mm")
                };
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error al obtener detalle completo del recordatorio: {ex.Message}");
        }

        return recordatorio;
    }

        public async Task<List<CitaViewModel>> ObtenerCitasDelDiaAsync(int veterinarioId)
        {
            var citas = new List<CitaViewModel>();
            var cultura = new CultureInfo("es-MX");

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var fechaHoy = DateTime.Today;
                var fechaManana = DateTime.Today.AddDays(1);

                var query = @"
                SELECT 
                    c.id, c.veterinarioID, c.mascotaID, c.tipoCitaID, c.fechaCita, 
                    c.importeAdicional, c.observaciones,
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
                WHERE c.fechaCita >= @fechaHoy 
                  AND c.fechaCita < @fechaManana 
                  AND c.veterinarioID = @veterinarioId
                ORDER BY c.fechaCita";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@fechaHoy", fechaHoy);
                command.Parameters.AddWithValue("@fechaManana", fechaManana);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var importeBase = reader["importe"] != DBNull.Value ? Convert.ToDecimal(reader["importe"]) : 0;
                    var importeAdicional = reader["importeAdicional"] != DBNull.Value ? Convert.ToDecimal(reader["importeAdicional"]) : 0;

                    citas.Add(new CitaViewModel
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
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener citas del veterinario: {ex.Message}");
            }

            return citas;
        }

        public async Task<List<RecordatorioViewModel>> ObtenerRecordatoriosHoyAsync(int veterinarioId, int cantidad = 5)
        {
            var recordatorios = new List<RecordatorioViewModel>();

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var fechaHoraActual = DateTime.Now;

                var query = @"
                            SELECT TOP (@cantidad)
                                r.id, r.citaID, r.fechaRecordatorio, r.asunto, r.mensaje,
                                d.nombre AS nombreDueno,
                                m.nombre AS nombreMascota,
                                d.numeroTelefono AS telefonoDueno,
                                d.email AS emailDueno
                            FROM RecordatoriosDueños r
                            INNER JOIN Citas c ON r.citaID = c.id
                            INNER JOIN Mascotas m ON c.mascotaID = m.id
                            INNER JOIN Dueños d ON m.dueñoID = d.id
                            WHERE r.fechaRecordatorio >= @fechaHoraActual
                              AND c.veterinarioID = @veterinarioId
                            ORDER BY r.fechaRecordatorio";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@cantidad", cantidad);
                command.Parameters.AddWithValue("@fechaHoraActual", fechaHoraActual);
                command.Parameters.AddWithValue("@veterinarioId", veterinarioId);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    recordatorios.Add(new RecordatorioViewModel
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        CitaID = Convert.ToInt32(reader["citaID"]),
                        FechaRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]),
                        Asunto = reader["asunto"]?.ToString(),
                        Mensaje = reader["mensaje"]?.ToString(),
                        NombreDueno = reader["nombreDueno"]?.ToString(),
                        NombreMascota = reader["nombreMascota"]?.ToString(),
                        TelefonoDueno = reader["telefonoDueno"]?.ToString(),
                        EmailDueno = reader["emailDueno"]?.ToString(),
                        HoraRecordatorio = Convert.ToDateTime(reader["fechaRecordatorio"]).ToString("HH:mm")
                    });
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al obtener recordatorios del veterinario: {ex.Message}");
            }

            return recordatorios;
        }
    }
}
