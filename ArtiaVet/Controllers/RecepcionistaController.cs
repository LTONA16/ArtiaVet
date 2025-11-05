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