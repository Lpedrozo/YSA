using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YSA.Web.Models.ViewModels
{
    public class GestionarProductosViewModel
    {
        // Lista de productos para la tabla
        public IEnumerable<ProductoViewModel> Productos { get; set; }

        // Propiedades para los dropdowns del modal de creación
        public CrearProductoViewModel NuevoProducto { get; set; }
        public IEnumerable<SelectListItem> AutoresDisponibles { get; set; }
        public IEnumerable<SelectListItem> CategoriasDisponibles { get; set; }
    }
    public class ProductoViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [Display(Name = "Tipo")]
        public string TipoProducto { get; set; }

        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C}")] 
        public decimal Precio { get; set; }

        [Display(Name = "Autor")]
        public string AutorNombre { get; set; }

        [Display(Name = "Categorías")]
        public string Categorias { get; set; }
    }
    public class CrearProductoViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [StringLength(255)]
        [Display(Name = "Descripción Corta")]
        public string DescripcionCorta { get; set; }

        [Display(Name = "Descripción Larga")]
        public string DescripcionLarga { get; set; }

        [Required(ErrorMessage = "El tipo de producto es obligatorio.")]
        [Display(Name = "Tipo de Producto")]
        public string TipoProducto { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "Imagen de Portada")]
        public IFormFile ImagenPortada { get; set; }

        [Required(ErrorMessage = "El archivo digital es obligatorio.")]
        [Display(Name = "Archivo Digital")]
        public IFormFile ArchivoDigital { get; set; }

        [Display(Name = "Autor")]
        public int? AutorId { get; set; }

        [Display(Name = "Categorías")]
        public List<int> CategoriaIds { get; set; }

        // Propiedades para las listas desplegables
        public IEnumerable<SelectListItem>? TiposProductoDisponibles { get; set; }
        public IEnumerable<SelectListItem>? AutoresDisponibles { get; set; }
        public IEnumerable<SelectListItem>? CategoriasDisponibles { get; set; }
    }
}