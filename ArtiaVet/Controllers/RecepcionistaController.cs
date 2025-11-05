using ArtiaVet.Models;
using ArtiaVet.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ArtiaVet.Controllers
{
    public class RecepcionistaController : Controller
    {
        private readonly IRepositorioDropdowns _repositorioDropdowns;
        private readonly IRepositorioInventario _repositorioInventario;

        public RecepcionistaController(
            IRepositorioDropdowns repositorioDropdowns,
            IRepositorioInventario repositorioInventario)
        {
            _repositorioDropdowns = repositorioDropdowns;
            _repositorioInventario = repositorioInventario;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Veterinarios = await _repositorioDropdowns.ObtenerVeterinariosAsync();
            ViewBag.Mascotas = await _repositorioDropdowns.ObtenerMascotasAsync();
            ViewBag.TiposCita = await _repositorioDropdowns.ObtenerTiposCitaAsync();
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

        // ============ MÉTODOS DE CALENDARIO ============

        public async Task<IActionResult> Calendario(int? mes, int? año, string fecha)
        {
            // Si no se especifica mes/año, usar el actual
            var fechaActual = DateTime.Now;

            // Determinar la fecha base
            DateTime fechaBase;
            if (!string.IsNullOrEmpty(fecha) && DateTime.TryParse(fecha, out DateTime fechaParam))
            {
                fechaBase = fechaParam;
            }
            else
            {
                fechaBase = fechaActual;
            }

            var mesSeleccionado = mes ?? fechaBase.Month;
            var añoSeleccionado = año ?? fechaBase.Year;

            // Calcular inicio y fin de la semana
            var inicioSemana = fechaBase.AddDays(-(int)fechaBase.DayOfWeek);
            var finSemana = inicioSemana.AddDays(6);

            // Obtener citas de toda la semana (puede abarcar dos meses)
            var citasMes1 = await _repositorioDropdowns.ObtenerCitasPorMesAsync(inicioSemana.Year, inicioSemana.Month);
            var citasMes2 = await _repositorioDropdowns.ObtenerCitasPorMesAsync(finSemana.Year, finSemana.Month);

            // Combinar y filtrar citas de la semana
            var todasCitas = citasMes1.Union(citasMes2)
                .Where(c => c.FechaCita.Date >= inicioSemana.Date && c.FechaCita.Date <= finSemana.Date)
                .ToList();

            // Cargar dropdowns para el modal de crear cita
            ViewBag.Veterinarios = await _repositorioDropdowns.ObtenerVeterinariosAsync();
            ViewBag.Mascotas = await _repositorioDropdowns.ObtenerMascotasAsync();
            ViewBag.TiposCita = await _repositorioDropdowns.ObtenerTiposCitaAsync();

            // Pasar datos a la vista
            ViewBag.MesActual = mesSeleccionado;
            ViewBag.AñoActual = añoSeleccionado;
            ViewBag.Citas = todasCitas;
            ViewBag.FechaBase = fechaBase.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetalleCita(int id)
        {
            var detalle = await _repositorioDropdowns.ObtenerDetalleCitaAsync(id);
            if (detalle == null)
            {
                return NotFound();
            }
            return Json(detalle);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCitaCalendario(CitaViewModel cita)
        {
            if (!ModelState.IsValid)
            {
                // Recargar dropdowns
                ViewBag.Veterinarios = await _repositorioDropdowns.ObtenerVeterinariosAsync();
                ViewBag.Mascotas = await _repositorioDropdowns.ObtenerMascotasAsync();
                ViewBag.TiposCita = await _repositorioDropdowns.ObtenerTiposCitaAsync();
                return View("Calendario");
            }

            try
            {
                await _repositorioDropdowns.CrearCitaAsync(cita);
                TempData["Mensaje"] = "Cita creada exitosamente";

                // Redirigir al mes de la cita creada
                return RedirectToAction("Calendario", new
                {
                    mes = cita.FechaCita.Month,
                    año = cita.FechaCita.Year
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la cita: " + ex.Message;
                return RedirectToAction("Calendario");
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
    }
}