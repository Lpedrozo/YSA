// Archivo: YSA.Web.Models.ViewModels/RecursoActividadViewModel.cs (NUEVO)

using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class RecursoActividadViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe especificar el tipo de entidad.")]
        public string TipoEntidad { get; set; } // "Curso", "Modulo", "Leccion"

        [Required(ErrorMessage = "Debe especificar el ID de la entidad.")]
        [Range(1, int.MaxValue, ErrorMessage = "ID de entidad inválido.")]
        public int EntidadId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(255)]
        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Debe especificar el tipo de recurso.")]
        public string TipoRecurso { get; set; } // "Actividad", "PDF", "Enlace", "ArchivoDescargable"

        public string? Url { get; set; }

        public bool RequiereEntrega { get; set; }

        // Campo opcional para subir un archivo (para PDFs, ArchivosDescargables o Actividades con plantilla)
        public IFormFile? Archivo { get; set; }

        // Propiedades adicionales para la vista:
        public string? EntidadNombre { get; set; } // Nombre del Curso/Módulo/Lección
        public EntregaActividadViewModel? EntregaEstudiante { get; set; }

    }
    public class EntregaActividadViewModel
    {
        public int Id { get; set; }
        public string UrlArchivoEntrega { get; set; } = string.Empty;
        public string? ComentarioEstudiante { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Calificado, Rechazado
        public decimal? Calificacion { get; set; }
        public string? ObservacionInstructor { get; set; }
        public DateTime? FechaCalificacion { get; set; }
    }
    public class EntregaActividadPendienteViewModel
    {
        public int EntregaId { get; set; }
        public string ActividadTitulo { get; set; } = string.Empty;
        public string CursoTitulo { get; set; } = string.Empty;
        public string EstudianteNombre { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
        public string UrlArchivoEntrega { get; set; } = string.Empty;
        public string? ComentarioEstudiante { get; set; }
        public string TipoEntidad { get; set; } = string.Empty; // Valor: "Curso", "Modulo" o "Leccion"
    }
}