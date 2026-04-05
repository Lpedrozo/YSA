using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class ClasePresencial
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }

        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [StringLength(1000)]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaHoraInicio { get; set; }

        [Required]
        public DateTime FechaHoraFin { get; set; }

        [StringLength(255)]
        public string Lugar { get; set; } = "Estudio de la Academia"; // Por defecto el estudio

        public int CapacidadMaxima { get; set; } = 20; // Capacidad máxima de estudiantes por clase

        // Estado de la clase: Programada, EnCurso, Completada, Cancelada
        [StringLength(50)]
        public string Estado { get; set; } = "Programada";

        // Notas adicionales para el instructor
        public string NotasInstructor { get; set; }

        // URL para meet si la clase es híbrida (opcional)
        [StringLength(500)]
        public string? UrlMeet { get; set; }

        // Relación con las inscripciones de estudiantes
        public virtual ICollection<InscripcionClase> Inscripciones { get; set; } = new List<InscripcionClase>();

        // Relación con recursos (PDFs, materiales) - Reutilizamos RecursoActividad
        // No es una FK directa, se maneja por polimorfismo con TipoEntidad = "ClasePresencial" y EntidadId = Id
    }
}