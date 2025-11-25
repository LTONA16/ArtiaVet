using ArtiaVet.Filters;
using ArtiaVet.Models;
using ArtiaVet.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ArtiaVet.Controllers
{
    [SoloVeterinario]
    public class VeterinarioController : Controller
    {
        private readonly IRepositorioCalendarioVeterinario _repositorioCalendario;
        private readonly IRepositorioCitasVeterinario _repositorioCitas;
        private readonly IRepositorioInventario _repositorioInventario;

        public VeterinarioController(
            IRepositorioCalendarioVeterinario repositorioCalendario,
            IRepositorioInventario repositorioInventario,
            IRepositorioCitasVeterinario repositorioCitas)
        {
            _repositorioCalendario = repositorioCalendario;
            _repositorioCitas = repositorioCitas;
            _repositorioInventario = repositorioInventario;
        }


        // ============ MÉTODOS DE INVENTARIO ============

        public async Task<IActionResult> Inventario()
        {
            ViewBag.Inventario = await _repositorioInventario.ObtenerInventarioAsync();
            ViewBag.Insumos = await _repositorioInventario.ObtenerInsumosAsync();
            ViewBag.StockBajo = await _repositorioInventario.ObtenerStockBajoAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInventario(InventarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Inventario");
            }

            try
            {
                await _repositorioInventario.CrearInventarioAsync(model);
                TempData["Mensaje"] = "Inventario creado exitosamente";
                return RedirectToAction("Inventario");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear inventario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear el inventario. Por favor intente nuevamente.";
                return RedirectToAction("Inventario");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarInventario(InventarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Inventario");
            }

            try
            {
                await _repositorioInventario.ActualizarInventarioAsync(model);
                TempData["Mensaje"] = "Inventario actualizado exitosamente";
                return RedirectToAction("Inventario");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar inventario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al actualizar el inventario. Por favor intente nuevamente.";
                return RedirectToAction("Inventario");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarInventario(int id)
        {
            try
            {
                await _repositorioInventario.EliminarInventarioAsync(id);
                TempData["Mensaje"] = "Inventario eliminado exitosamente";
                return RedirectToAction("Inventario");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar inventario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al eliminar el inventario. Por favor intente nuevamente.";
                return RedirectToAction("Inventario");
            }
        }

        // ============ MÉTODOS DE CALENDARIO ============

        public async Task<IActionResult> Calendario(DateTime? fecha)
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return RedirectToAction("Login", "Acceso");
                }

                var fechaConsulta = fecha ?? DateTime.Today;
                var calendarioSemanal = await _repositorioCalendario.ObtenerCalendarioSemanalAsync(fechaConsulta, veterinarioId.Value);
                
                return View(calendarioSemanal);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar calendario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al cargar el calendario. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleCita(int id)
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión no válida" });
                }

                var cita = await _repositorioCalendario.ObtenerDetalleCitaAsync(id, veterinarioId.Value);
                
                if (cita == null)
                {
                    return NotFound(new { mensaje = "Cita no encontrada" });
                }
                
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = cita.Id,
                        veterinario = cita.NombreVeterinario,
                        mascota = cita.NombreMascota,
                        dueno = cita.NombreDueno,
                        tipoCita = cita.TipoCita,
                        fecha = cita.FechaCita.ToString("dd/MM/yyyy"),
                        horaInicio = cita.HoraInicio,
                        horaFin = cita.HoraFin,
                        importeTotal = cita.ImporteTotal,
                        importeAdicional = cita.ImporteAdicional,
                        observaciones = cita.Observaciones,
                        colorFondo = cita.ColorFondo,
                        colorTexto = cita.ColorTexto
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle de cita: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener los detalles de la cita" });
            }
        }

        // ============ MÉTODOS DE CITAS ============

        public async Task<IActionResult> Citas()
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return RedirectToAction("Login", "Acceso");
                }

                var citasProximas = await _repositorioCitas.ObtenerCitasProximos7DiasAsync(veterinarioId.Value);
                ViewBag.Mascotas = await _repositorioCitas.ObtenerMascotasAsync();
                ViewBag.TiposCita = await _repositorioCitas.ObtenerTiposCitaAsync();
                ViewBag.TiposAnimales = await _repositorioCitas.ObtenerTiposAnimalesAsync();
                ViewBag.Duenos = await _repositorioCitas.ObtenerDuenosAsync();
                ViewBag.Alergias = await _repositorioCitas.ObtenerAlergiasAsync();
                
                return View(citasProximas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar citas: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al cargar las citas. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(CitaViewModel model)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Asignar el veterinario autenticado
            model.VeterinarioID = veterinarioId.Value;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Citas");
            }

            try
            {
                await _repositorioCitas.CrearCitaAsync(model);
                TempData["Mensaje"] = "Cita creada exitosamente";
                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear cita: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear la cita. Por favor intente nuevamente.";
                return RedirectToAction("Citas");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCitaDesdeListado(CitaViewModel model)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            // Asignar el veterinario autenticado
            model.VeterinarioID = veterinarioId.Value;
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Citas");
            }

            try
            {
                await _repositorioCitas.CrearCitaAsync(model);
                TempData["Mensaje"] = "Cita creada exitosamente";
                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear cita: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear la cita. Por favor intente nuevamente.";
                return RedirectToAction("Citas");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCita(CitaViewModel model)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Citas");
            }

            try
            {
                await _repositorioCitas.ActualizarCitaAsync(model, veterinarioId.Value);
                TempData["Mensaje"] = "Cita actualizada exitosamente";
                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar cita: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al actualizar la cita. Por favor intente nuevamente.";
                return RedirectToAction("Citas");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCita(int id)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            try
            {
                await _repositorioCitas.EliminarCitaAsync(id, veterinarioId.Value);
                TempData["Mensaje"] = "Cita eliminada exitosamente";
                return RedirectToAction("Citas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar cita: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al eliminar la cita. Por favor intente nuevamente.";
                return RedirectToAction("Citas");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearDueno([FromBody] DuenoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, mensaje = string.Join(", ", errors) });
            }

            try
            {
                var dueñoId = await _repositorioCitas.CrearDuenoAsync(model);
                var dueños = await _repositorioCitas.ObtenerDuenosAsync();
                
                return Json(new { 
                    success = true, 
                    mensaje = "Dueño creado exitosamente",
                    dueñoId = dueñoId,
                    dueños = dueños
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear dueño: {ex.Message}");
                return Json(new { success = false, mensaje = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearMascota([FromBody] MascotaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, mensaje = string.Join(", ", errors) });
            }

            try
            {
                var mascotaId = await _repositorioCitas.CrearMascotaAsync(model);
                var mascotas = await _repositorioCitas.ObtenerMascotasAsync();
                
                return Json(new { 
                    success = true, 
                    mensaje = "Mascota creada exitosamente",
                    mascotaId = mascotaId,
                    mascotas = mascotas
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear mascota: {ex.Message}");
                return Json(new { success = false, mensaje = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleCitaCompleto(int id)
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión no válida" });
                }

                var cita = await _repositorioCitas.ObtenerDetalleCitaAsync(id, veterinarioId.Value);
                
                if (cita == null)
                {
                    return NotFound(new { mensaje = "Cita no encontrada" });
                }
                
                return Json(new { success = true, data = cita });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle de cita: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener los detalles de la cita" });
            }
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerDuenos()
        {
            try
            {
                var dueños = await _repositorioCitas.ObtenerDuenosAsync();
                return Json(new { success = true, dueños });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener dueños: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener dueños" });
            }
        }

        // ============ MÉTODOS DE RECORDATORIOS ============

        public async Task<IActionResult> Recordatorios()
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return RedirectToAction("Login", "Acceso");
                }

                var recordatoriosViewModel = await _repositorioCitas.ObtenerRecordatoriosProximos7DiasAsync(veterinarioId.Value);
                ViewBag.Citas = await _repositorioCitas.ObtenerCitasActivasAsync(veterinarioId.Value);
                return View(recordatoriosViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar recordatorios: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al cargar los recordatorios. Por favor intente nuevamente.";
                return View(new RecordatoriosProximosViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCitasActivas()
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión no válida" });
                }

                var citas = await _repositorioCitas.ObtenerCitasActivasAsync(veterinarioId.Value);
                return Json(new { success = true, data = citas });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener citas activas: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener las citas activas" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRecordatorio(RecordatorioViewModel model)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Recordatorios");
            }

            try
            {
                await _repositorioCitas.CrearRecordatorioAsync(model, veterinarioId.Value);
                TempData["Mensaje"] = "Recordatorio creado exitosamente";
                return RedirectToAction("Recordatorios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear recordatorio: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear el recordatorio. Por favor intente nuevamente.";
                return RedirectToAction("Recordatorios");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerRecordatorio(int id)
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión no válida" });
                }

                var recordatorio = await _repositorioCitas.ObtenerRecordatorioPorIdAsync(id, veterinarioId.Value);

                if (recordatorio == null)
                {
                    return NotFound(new { mensaje = "Recordatorio no encontrado" });
                }

                return Json(new { success = true, data = recordatorio });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener recordatorio: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener el recordatorio" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecordatorio(RecordatorioViewModel model)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Recordatorios");
            }

            try
            {
                await _repositorioCitas.ActualizarRecordatorioAsync(model, veterinarioId.Value);
                TempData["Mensaje"] = "Recordatorio actualizado exitosamente";
                return RedirectToAction("Recordatorios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al actualizar recordatorio: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al actualizar el recordatorio. Por favor intente nuevamente.";
                return RedirectToAction("Recordatorios");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRecordatorio(int id)
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Acceso");
            }

            try
            {
                await _repositorioCitas.EliminarRecordatorioAsync(id, veterinarioId.Value);
                TempData["Mensaje"] = "Recordatorio eliminado exitosamente";
                return RedirectToAction("Recordatorios");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar recordatorio: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al eliminar el recordatorio. Por favor intente nuevamente.";
                return RedirectToAction("Recordatorios");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleRecordatorio(int id)
        {
            try
            {
                var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!veterinarioId.HasValue)
                {
                    return Json(new { success = false, mensaje = "Sesión no válida" });
                }

                var recordatorio = await _repositorioCitas.ObtenerDetalleRecordatorioCompletoAsync(id, veterinarioId.Value);

                if (recordatorio == null)
                {
                    return NotFound(new { mensaje = "Recordatorio no encontrado" });
                }

                return Json(new { success = true, data = recordatorio });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle del recordatorio: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener los detalles del recordatorio" });
            }
        }

        public async Task<IActionResult> Index()
        {
            var veterinarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (!veterinarioId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Usamos los métodos del repositorio específico
            ViewBag.CitasHoy = await _repositorioCitas.ObtenerCitasDelDiaAsync(veterinarioId.Value);
            ViewBag.RecordatoriosHoy = await _repositorioCitas.ObtenerRecordatoriosHoyAsync(veterinarioId.Value, 5);

            // Inventario sigue siendo general
            ViewBag.StockBajo = await _repositorioInventario.ObtenerTop5StockBajoAsync();

            return View();
        }
    }
}