using Microsoft.AspNetCore.Mvc.Rendering;

namespace YSA.Web.Models.ViewModels
{
    public class PaqueteViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string DescripcionLarga { get; set; }
        public decimal Precio { get; set; }
        public string? UrlImagen { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public bool EsDestacado { get; set; }
        public bool EsRecomendado { get; set; }

        // Propiedades para mostrar
        public List<string> CursosNombres { get; set; } = new List<string>();
        public List<string> ProductosNombres { get; set; } = new List<string>();
        public int TotalCursos { get; set; }
        public int TotalProductos { get; set; }
        public decimal Ahorro { get; set; }

        // Para creación/edición
        public List<int> CursosSeleccionados { get; set; } = new List<int>();
        public List<int> ProductosSeleccionados { get; set; } = new List<int>();

        // Para selects
        public List<SelectListItem> CursosDisponibles { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> ProductosDisponibles { get; set; } = new List<SelectListItem>();
    }

    public class PaqueteListaViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public decimal PrecioTotalItems { get; set; }
        public decimal Ahorro { get; set; }
        public int CantidadItems { get; set; }
        public bool EsDestacado { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}