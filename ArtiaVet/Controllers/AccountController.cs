using Microsoft.AspNetCore.Mvc;
using ArtiaVet.Servicios;
using ArtiaVet.Models;

namespace ArtiaVet.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepositorioUsuarios _repositorioUsuarios;

        public AccountController(IRepositorioUsuarios repositorioUsuarios)
        {
            _repositorioUsuarios = repositorioUsuarios;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir según su rol
            var userId = HttpContext.Session.GetInt32("UsuarioId");
            if (userId.HasValue)
            {
                return RedirectToHomePorRol();
            }

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            Console.WriteLine($"=== INICIO LOGIN ===");
            Console.WriteLine($"Username recibido: {username}");
            Console.WriteLine($"Password recibido: {!string.IsNullOrEmpty(password)}");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Error: Campos vacíos");
                TempData["Error"] = "Por favor ingrese usuario y contraseña";
                return View();
            }

            try
            {
                Console.WriteLine("Llamando a ValidarCredencialesAsync...");
                var usuario = await _repositorioUsuarios.ValidarCredencialesAsync(username, password);

                Console.WriteLine($"Usuario obtenido: {usuario != null}");

                if (usuario == null)
                {
                    Console.WriteLine("Usuario NULL - credenciales incorrectas");
                    TempData["Error"] = "Usuario o contraseña incorrectos";
                    return View();
                }

                Console.WriteLine($"Usuario válido: {usuario.Nombre}, Tipo: {usuario.TipoUsuario}");

                // Guardar información del usuario en la sesión
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetInt32("TipoUsuario", usuario.TipoUsuario);
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? "");
                HttpContext.Session.SetString("Username", usuario.Username ?? "");
                
                if (usuario.ImagenBase64 != null)
                {
                    HttpContext.Session.SetString("UsuarioImagen", usuario.ImagenBase64);
                }

                Console.WriteLine("Sesión guardada correctamente");
                Console.WriteLine($"Redirigiendo según tipo usuario: {usuario.TipoUsuario}");

                TempData["Mensaje"] = $"¡Bienvenido {usuario.Nombre}!";

                // Redirigir según el tipo de usuario
                var redirect = RedirectToHomePorRol();
                Console.WriteLine($"=== FIN LOGIN - REDIRECT ===");
                return redirect;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR EN LOGIN ===");
                Console.WriteLine($"Error en login: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                TempData["Error"] = "Ocurrió un error al iniciar sesión. Por favor intente nuevamente.";
                return View();
            }
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Mensaje"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login");
        }

        // GET: Account/AccesoDenegado
        public IActionResult AccesoDenegado()
        {
            ViewBag.Mensaje = "No tienes permisos para acceder a esta sección";
            return View();
        }

        // Método privado para redirigir según el rol
        private IActionResult RedirectToHomePorRol()
        {
            var tipoUsuario = HttpContext.Session.GetInt32("TipoUsuario");

            return tipoUsuario switch
            {
                TiposUsuario.Veterinario => RedirectToAction("Index", "Veterinario"),
                TiposUsuario.Recepcionista => RedirectToAction("Index", "Recepcionista"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}