using ArtiaVet.Filters;
using ArtiaVet.Models;
using ArtiaVet.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ArtiaVet.Controllers
{
    [SoloRecepcionista]
    public class RecepcionistaController : Controller
    {
        private readonly IRepositorioDropdowns _repositorioDropdowns;
        private readonly IRepositorioInventario _repositorioInventario;
        private readonly IRepositorioCalendario _repositorioCalendario;
        private readonly IRepositorioCitas _repositorioCitas;
        private readonly IRepositorioFacturas _repositorioFacturas;

        public RecepcionistaController(
            IRepositorioDropdowns repositorioDropdowns,
            IRepositorioInventario repositorioInventario,
            IRepositorioCalendario repositorioCalendario,
            IRepositorioCitas repositorioCitas,
            IRepositorioFacturas repositorioFacturas)
        {
            _repositorioDropdowns = repositorioDropdowns;
            _repositorioInventario = repositorioInventario;
            _repositorioCalendario = repositorioCalendario;
            _repositorioCitas = repositorioCitas;
            _repositorioFacturas = repositorioFacturas;
        }

        public async Task<IActionResult> Index()
        {
            //// Datos para el modal de crear cita
            //ViewBag.Veterinarios = await _repositorioDropdowns.ObtenerVeterinariosAsync();
            //ViewBag.Mascotas = await _repositorioDropdowns.ObtenerMascotasAsync();
            //ViewBag.TiposCita = await _repositorioDropdowns.ObtenerTiposCitaAsync();

            // Datos para las vistas parciales dinámicas
            ViewBag.CitasHoy = await _repositorioCitas.ObtenerCitasDelDiaAsync();
            ViewBag.RecordatoriosHoy = await _repositorioCitas.ObtenerRecordatoriosHoyAsync(5);
            ViewBag.StockBajo = await _repositorioInventario.ObtenerTop5StockBajoAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCita(CitaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Veterinarios = await _repositorioDropdowns.ObtenerVeterinariosAsync();
                ViewBag.Mascotas = await _repositorioDropdowns.ObtenerMascotasAsync();
                ViewBag.TiposCita = await _repositorioDropdowns.ObtenerTiposCitaAsync();
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Index");
            }

            try
            {
                await _repositorioDropdowns.CrearCitaAsync(model);
                TempData["Mensaje"] = "Cita creada exitosamente";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear cita: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear la cita. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
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
            // Limpiamos errores de validación de campos que no vienen en el form (Nombre, Precio)
            // Aunque con el cambio en el Modelo esto ya debería estar resuelto.
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
            catch (InvalidOperationException ex)
            {
                // CAPTURAMOS EL ERROR DE DUPLICADO QUE CREAMOS EN EL REPOSITORIO
                TempData["Error"] = ex.Message; // Mostrará: "Este insumo ya está registrado..."
                return RedirectToAction("Inventario");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear inventario: {ex.Message}");
                TempData["Error"] = "Ocurrió un error inesperado. Por favor intente nuevamente.";
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
                var fechaConsulta = fecha ?? DateTime.Today;
                var calendarioSemanal = await _repositorioCalendario.ObtenerCalendarioSemanalAsync(fechaConsulta);
                var veterinarios = await _repositorioCalendario.ObtenerVeterinariosConCitasAsync(
                    calendarioSemanal.FechaInicio,
                    calendarioSemanal.FechaFin
                );

                ViewBag.Veterinarios = veterinarios;
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
                var cita = await _repositorioCalendario.ObtenerDetalleCitaAsync(id);

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
                var citasProximas = await _repositorioCitas.ObtenerCitasProximos7DiasAsync();
                ViewBag.Veterinarios = await _repositorioCitas.ObtenerVeterinariosAsync();
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
        public async Task<IActionResult> CrearCitaDesdeListado(CitaViewModel model)
        {
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

                return Json(new
                {
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

                return Json(new
                {
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
                var cita = await _repositorioCitas.ObtenerDetalleCitaAsync(id);

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
                // devolver en el mismo formato que usas en el frontend: { dueños: [ { value, text } ] }
                return Json(new { success = true, dueños });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener dueños: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener dueños" });
            }
        }


        // Recordatorios
        // Agregar estos métodos a tu RecepcionistaController existente

        // GET: Recepcionista/Recordatorios
        public async Task<IActionResult> Recordatorios()
        {
            try
            {
                var recordatoriosViewModel = await _repositorioCitas.ObtenerRecordatoriosProximos7DiasAsync();
                ViewBag.Citas = await _repositorioCitas.ObtenerCitasActivasAsync();
                return View(recordatoriosViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar recordatorios: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al cargar los recordatorios. Por favor intente nuevamente.";
                return View(new RecordatoriosProximosViewModel());
            }
        }

        // GET: Recepcionista/ObtenerCitasActivas
        [HttpGet]
        public async Task<IActionResult> ObtenerCitasActivas()
        {
            try
            {
                var citas = await _repositorioCitas.ObtenerCitasActivasAsync();
                return Json(new { success = true, data = citas });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener citas activas: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener las citas activas" });
            }
        }

        // POST: Recepcionista/CrearRecordatorio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearRecordatorio(RecordatorioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Recordatorios");
            }

            try
            {
                await _repositorioCitas.CrearRecordatorioAsync(model);
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

        // GET: Recepcionista/ObtenerRecordatorio
        [HttpGet]
        public async Task<IActionResult> ObtenerRecordatorio(int id)
        {
            try
            {
                var recordatorio = await _repositorioCitas.ObtenerRecordatorioPorIdAsync(id);

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

        // POST: Recepcionista/EditarRecordatorio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarRecordatorio(RecordatorioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Recordatorios");
            }

            try
            {
                await _repositorioCitas.ActualizarRecordatorioAsync(model);
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

        // POST: Recepcionista/EliminarRecordatorio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarRecordatorio(int id)
        {
            try
            {
                await _repositorioCitas.EliminarRecordatorioAsync(id);
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

        // GET: Recepcionista/ObtenerDetalleRecordatorio
        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleRecordatorio(int id)
        {
            try
            {
                var recordatorio = await _repositorioCitas.ObtenerDetalleRecordatorioCompletoAsync(id);

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

        public async Task<IActionResult> Facturas(string? busqueda, DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                List<FacturaViewModel> facturas;

                if (!string.IsNullOrWhiteSpace(busqueda) || fechaInicio.HasValue || fechaFin.HasValue)
                {
                    facturas = await _repositorioFacturas.BuscarFacturasAsync(busqueda, fechaInicio, fechaFin);
                }
                else
                {
                    facturas = await _repositorioFacturas.ObtenerFacturasAsync();
                }

                var viewModel = new FacturasViewModel
                {
                    Facturas = facturas,
                    TerminoBusqueda = busqueda,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                };

                ViewBag.CitasSinFacturar = await _repositorioFacturas.ObtenerCitasSinFacturarAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar facturas: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al cargar las facturas. Por favor intente nuevamente.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearFactura(FacturaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor complete todos los campos requeridos";
                return RedirectToAction("Facturas");
            }

            try
            {
                await _repositorioFacturas.CrearFacturaAsync(model);
                TempData["Mensaje"] = "Factura creada exitosamente";
                return RedirectToAction("Facturas");
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Facturas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear factura: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al crear la factura. Por favor intente nuevamente.";
                return RedirectToAction("Facturas");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleFactura(int id)
        {
            try
            {
                var factura = await _repositorioFacturas.ObtenerFacturaPorIdAsync(id);

                if (factura == null)
                {
                    return NotFound(new { mensaje = "Factura no encontrada" });
                }

                return Json(new { success = true, data = factura });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener detalle de factura: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener los detalles de la factura" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarFactura(int id)
        {
            try
            {
                await _repositorioFacturas.EliminarFacturaAsync(id);
                TempData["Mensaje"] = "Factura eliminada exitosamente";
                return RedirectToAction("Facturas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar factura: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al eliminar la factura. Por favor intente nuevamente.";
                return RedirectToAction("Facturas");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCitasSinFacturar()
        {
            try
            {
                var citas = await _repositorioFacturas.ObtenerCitasSinFacturarAsync();
                return Json(new { success = true, data = citas });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener citas sin facturar: {ex.Message}");
                return Json(new { success = false, mensaje = "Error al obtener las citas sin facturar" });
            }
        }
    }
}