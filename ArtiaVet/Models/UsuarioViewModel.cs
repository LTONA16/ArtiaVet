using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    public class UsuarioViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string? Username { get; set; }
        
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string? Password { get; set; }
        
        [Required(ErrorMessage = "El nombre es requerido")]
        public string? Nombre { get; set; }
        
        public int TipoUsuario { get; set; }
        
        public string? TipoUsuarioNombre { get; set; }
        
        public byte[]? Imagen { get; set; }
        
        // Propiedad helper para la imagen en Base64
        public string? ImagenBase64 => Imagen != null 
            ? $"data:image/png;base64,{Convert.ToBase64String(Imagen)}" 
            : null;

        // Propiedades helper para roles
        public bool EsVeterinario => TipoUsuario == 2;
        public bool EsRecepcionista => TipoUsuario == 3;
    }

    // Constantes para tipos de usuario
    public static class TiposUsuario
    {
        public const int Veterinario = 2;
        public const int Recepcionista = 3;
    }
}