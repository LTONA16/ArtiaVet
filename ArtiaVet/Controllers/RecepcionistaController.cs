using ArtiaVet.Models;
using ArtiaVet.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ArtiaVet.Controllers  // Asegúrate de que tu controlador esté en este namespace
{
    public class RecepcionistaController : Controller
    {
        private readonly IRepositorioDropdowns _repositorioDropdowns;

        public RecepcionistaController(IRepositorioDropdowns repositorioDropdowns)
        {
            _repositorioDropdowns = repositorioDropdowns;
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
    }
}