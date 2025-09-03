using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace YSA.Web.Models.ViewModels
{
    public class CursoViewModel
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

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "URL de la Imagen")]
        public string? UrlImagen { get; set; }

        public int? InstructorId { get; set; }

        [Display(Name = "Categorías")]
        public int[] CategoriasSeleccionadas { get; set; }

        public IEnumerable<SelectListItem>? ListaCategorias { get; set; }
    }

    public class CrearCursoPostViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionCorta { get; set; }
        public string DescripcionLarga { get; set; }
        public decimal Precio { get; set; }
        public string UrlImagen { get; set; }
        public int? InstructorId { get; set; }
        public int[] CategoriasSeleccionadas { get; set; }
    }
    public class CursoCompletoViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string DescripcionLarga { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public bool TieneAcceso { get; set; }
        public bool EstaEnValidacion { get; set; }
        public List<ModuloConLeccionesViewModel> Modulos { get; set; }
    }
}