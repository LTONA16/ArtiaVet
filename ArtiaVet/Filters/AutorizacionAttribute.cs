using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ArtiaVet.Models;

namespace ArtiaVet.Filters
{
    // Atributo base para verificar que el usuario esté autenticado
    public class AutenticadoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UsuarioId");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Atributo para verificar que el usuario sea Veterinario
    public class SoloVeterinarioAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UsuarioId");
            var tipoUsuario = context.HttpContext.Session.GetInt32("TipoUsuario");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (tipoUsuario != TiposUsuario.Veterinario)
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Atributo para verificar que el usuario sea Recepcionista
    public class SoloRecepcionistaAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UsuarioId");
            var tipoUsuario = context.HttpContext.Session.GetInt32("TipoUsuario");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (tipoUsuario != TiposUsuario.Recepcionista)
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    // Atributo para verificar múltiples roles
    public class AutorizarRolesAttribute : ActionFilterAttribute
    {
        private readonly int[] _rolesPermitidos;

        public AutorizarRolesAttribute(params int[] roles)
        {
            _rolesPermitidos = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userId = context.HttpContext.Session.GetInt32("UsuarioId");
            var tipoUsuario = context.HttpContext.Session.GetInt32("TipoUsuario");

            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (!_rolesPermitidos.Contains(tipoUsuario.Value))
            {
                context.Result = new RedirectToActionResult("AccesoDenegado", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}