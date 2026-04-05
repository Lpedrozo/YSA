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

        [Display(Name = "Descripción corta")]
        public string DescripcionCorta { get; set; }

        [Display(Name = "Descripción larga")]
        public string DescripcionLarga { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "URL de la imagen")]
        public string? UrlImagen { get; set; }
        [Required(ErrorMessage = "Debe seleccionar el tipo de curso")]
        public TipoCurso TipoCurso { get; set; } = TipoCurso.Digital;
        public int? InstructorId { get; set; }

        [Display(Name = "Categorías")]
        public int[] CategoriasSeleccionadas { get; set; }

        public IEnumerable<SelectListItem>? ListaCategorias { get; set; }
        public bool EsDestacado { get; set; }
        public bool EsRecomendado { get; set; }
        public string? NombreInstructor { get; set; }
        public NivelDificultad Nivel { get; set; } // Añadir esta propiedad
        public Artista? Instructor { get; set; } // Agrega esta propiedad
        public List<string> InstructoresNombres { get; set; } = new List<string>();

    }
    public class ClasePresencialViewModel
    {
        public int Id { get; set; }
        public int CursoId { get; set; }

        [Required(ErrorMessage = "El título de la clase es obligatorio")]
        [StringLength(255)]
        public string Titulo { get; set; }

        [StringLength(1000)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio es obligatoria")]
        public DateTime FechaHoraInicio { get; set; }

        [Required(ErrorMessage = "La fecha y hora de fin es obligatoria")]
        public DateTime FechaHoraFin { get; set; }

        [StringLength(255)]
        public string Lugar { get; set; } = "Estudio de la Academia";

        [Range(1, 100)]
        public int CapacidadMaxima { get; set; } = 20;

        public string Estado { get; set; } = "Programada";
        public string NotasInstructor { get; set; }
        public string? UrlMeet { get; set; }

        // Para mostrar en listas
        public int InscritosCount { get; set; }
    }
    public class CursoIndexViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255)]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        [StringLength(255)]
        [Display(Name = "Descripción corta")]
        public string DescripcionCorta { get; set; }

        [Display(Name = "Descripción larga")]
        public string DescripcionLarga { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que 0.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Display(Name = "URL de la imagen")]
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
    public class ClasePresencialDetalleViewModel
    {
        public int ClaseId { get; set; }
        public int CursoId { get; set; }
        public string CursoTitulo { get; set; }
        public string ClaseTitulo { get; set; }
        public string ClaseDescripcion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public string Lugar { get; set; }
        public int CapacidadMaxima { get; set; }
        public int VacantesDisponibles { get; set; }
        public string UrlImagen { get; set; }
        public decimal Precio { get; set; }
        public string Estado { get; set; }
        public string? UrlMeet { get; set; }

        // Estado del usuario
        public bool UsuarioLogueado { get; set; }
        public bool PerfilCompleto { get; set; }
        public bool YaInscrito { get; set; }
        public bool TienePedidoPendiente { get; set; }
        public string UsuarioNombre { get; set; }
        public string UsuarioApellido { get; set; }
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
        public List<RecursoActividadViewModel> Actividades { get; set; } = new List<RecursoActividadViewModel>();
        public List<RecursoActividadViewModel> Recursos { get; set; } = new List<RecursoActividadViewModel>();
    }

    public class ResenaViewModel
    {
        public string NombreUsuario { get; set; }
        public int Calificacion { get; set; } // Ejemplo: 1 a 5
        public string Comentario { get; set; }
        public DateTime Fecha { get; set; }
        public int EstudianteId { get; set; }
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
        public int EstudianteId { get; set; }
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
    public class PreguntaPendienteViewModel
    {
        public int Id { get; set; }
        public string CursoTitulo { get; set; }
        public string EstudianteNombre { get; set; }
        public string Pregunta { get; set; }
        public DateTime FechaPregunta { get; set; }
    }
    public class GestionClasesCursoViewModel
    {
        public int CursoId { get; set; }
        public string CursoTitulo { get; set; }
        public List<ClaseConInscripcionesViewModel> Clases { get; set; } = new List<ClaseConInscripcionesViewModel>();
    }
    public class ActualizarAsistenciaDto
    {
        public int InscripcionId { get; set; }
        public string EstadoAsistencia { get; set; }
    }
    public class CrearClaseDto
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public string Lugar { get; set; }
        public string UrlMeet { get; set; }
    }
    public class EditarClaseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public string Lugar { get; set; }
        public string Estado { get; set; }
        public string UrlMeet { get; set; }
    }
    public class ClaseConInscripcionesViewModel
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraFin { get; set; }
        public int CapacidadMaxima { get; set; }
        public string Lugar { get; set; }
        public string Estado { get; set; }
        public string? UrlMeet { get; set; }
        public int InscritosCount { get; set; }
        public List<InscripcionClaseViewModel> Inscripciones { get; set; } = new List<InscripcionClaseViewModel>();
    }

    public class InscripcionClaseViewModel
    {
        public int Id { get; set; }
        public int EstudianteId { get; set; }
        public string EstudianteNombre { get; set; }
        public string EstudianteEmail { get; set; }
        public string EstadoAsistencia { get; set; }
        public DateTime FechaInscripcion { get; set; }
    }
}