// using ArtiaVet.Models;
// using ArtiaVet.Servicios;
// using Microsoft.AspNetCore.Mvc;

// namespace ArtiaVet.Controllers
// {
//     public class InventarioController : Controller
//     {
//         private readonly IRepositorioInventario _repositorioInventario;

//         public InventarioController(IRepositorioInventario repositorioInventario)
//         {
//             _repositorioInventario = repositorioInventario;
//         }

//         public async Task<IActionResult> Index()
//         {
//             ViewBag.Inventario = await _repositorioInventario.ObtenerInventarioAsync();
//             ViewBag.Insumos = await _repositorioInventario.ObtenerInsumosAsync();
//             ViewBag.StockBajo = await _repositorioInventario.ObtenerStockBajoAsync();
//             return View();
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Crear(InventarioViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 TempData["Error"] = "Por favor complete todos los campos requeridos";
//                 return RedirectToAction("Index");
//             }

//             try
//             {
//                 await _repositorioInventario.CrearInventarioAsync(model);
//                 TempData["Mensaje"] = "Inventario creado exitosamente";
//                 return RedirectToAction("Index");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error al crear inventario: {ex.Message}");
//                 TempData["Error"] = "Ocurrió un error al crear el inventario. Por favor intente nuevamente.";
//                 return RedirectToAction("Index");
//             }
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Actualizar(InventarioViewModel model)
//         {
//             if (!ModelState.IsValid)
//             {
//                 TempData["Error"] = "Por favor complete todos los campos requeridos";
//                 return RedirectToAction("Index");
//             }

//             try
//             {
//                 await _repositorioInventario.ActualizarInventarioAsync(model);
//                 TempData["Mensaje"] = "Inventario actualizado exitosamente";
//                 return RedirectToAction("Index");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error al actualizar inventario: {ex.Message}");
//                 TempData["Error"] = "Ocurrió un error al actualizar el inventario. Por favor intente nuevamente.";
//                 return RedirectToAction("Index");
//             }
//         }

//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Eliminar(int id)
//         {
//             try
//             {
//                 await _repositorioInventario.EliminarInventarioAsync(id);
//                 TempData["Mensaje"] = "Inventario eliminado exitosamente";
//                 return RedirectToAction("Index");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Error al eliminar inventario: {ex.Message}");
//                 TempData["Error"] = "Ocurrió un error al eliminar el inventario. Por favor intente nuevamente.";
//                 return RedirectToAction("Index");
//             }
//         }
//     }
// }