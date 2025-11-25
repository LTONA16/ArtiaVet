using Microsoft.AspNetCore.Mvc.ModelBinding.Validation; // Importante: Agrega este using
using System.ComponentModel.DataAnnotations;

namespace ArtiaVet.Models
{
    public class InventarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El insumo es requerido")]
        public int InsumoID { get; set; }

        [ValidateNever]
        public string? NombreInsumo { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(0, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor o igual a 0")]
        public int Cantidad { get; set; }

        [ValidateNever]
        public decimal PrecioUnitario { get; set; }

        public decimal ValorTotal => Cantidad * PrecioUnitario;

        public bool StockBajo => Cantidad < 50;
        public bool StockCritico => Cantidad < 20;
    }
}