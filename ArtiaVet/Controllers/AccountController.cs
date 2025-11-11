using Microsoft.AspNetCore.Mvc;

namespace ArtiaVet.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Por ahora, sin funcionalidad real
            // TODO: Implementar autenticación real
            
            // Simulación de login exitoso (temporal)
            TempData["Message"] = "Login exitoso (simulado)";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public IActionResult Register(string name, string email, string password)
        {
            // Por ahora, sin funcionalidad real
            // TODO: Implementar registro real
            
            TempData["Message"] = "Registro exitoso (simulado)";
            return RedirectToAction("Login");
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // TODO: Implementar logout real
            TempData["Message"] = "Sesión cerrada";
            return RedirectToAction("Index", "Home");
        }
    }
}