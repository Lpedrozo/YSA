using System.ComponentModel.DataAnnotations;
using YSA.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using YSA.Core.Enums;

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
        public bool EsDestacado { get; set; }
        public bool EsRecomendado { get; set; }
        public string? NombreInstructor { get; set; }
        public NivelDificultad Nivel { get; set; } // Añadir esta propiedad
        public Artista Instructor { get; set; } // Agrega esta propiedad

    }

    public class CursoIndexViewModel
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

        public List<string> ListaCategorias { get; set; }
        public NivelDificultad Nivel { get; set; } // Añadir esta propiedad

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
        public string DescripcionCorta { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public bool TieneAcceso { get; set; }
        public bool EstaEnValidacion { get; set; }
        public List<ModuloConLeccionesViewModel> Modulos { get; set; }
        public List<ResenaViewModel> Resenas { get; set; } = new List<ResenaViewModel>();
        public List<PreguntaRespuestaViewModel> Preguntas { get; set; }
        public List<AnuncioViewModel> Anuncios { get; set; }
        public List<Curso> CursosDestacados { get; set; }


    }

    public class ResenaViewModel
    {
        public string NombreUsuario { get; set; }
        public int Calificacion { get; set; } // Ejemplo: 1 a 5
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
    }
    public class CursosIndexViewModel
    {
        public List<CursoIndexViewModel> Cursos { get; set; }
        public List<CursoIndexViewModel> CursosDestacados { get; set; }
        public List<CursoIndexViewModel> CursosRecomendados { get; set; }
        public List<string> CategoriasDisponibles { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string CategoriaActual { get; set; }
    }
    public class PreguntaRespuestaViewModel
    {
        public int Id { get; set; }
        public string Pregunta { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime FechaPregunta { get; set; }

        // Propiedades de la respuesta
        public string Respuesta { get; set; }
        public string NombreInstructor { get; set; }
        public DateTime? FechaRespuesta { get; set; }
    }
    public class AnuncioViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        public string Contenido { get; set; }

        public int CursoId { get; set; }

        public DateTime FechaPublicacion { get; set; }
    }
    public class AsociarArtistaViewModel
    {
        public int CursoId { get; set; }
        public int InstructorId { get; set; }
    }
}