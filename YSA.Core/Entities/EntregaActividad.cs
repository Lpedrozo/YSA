using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using YSA.Core.Entities;
using YSA.Core.Enums;

namespace YSA.Core.Entities
{
    public class EntregaActividad
    {
        public int Id { get; set; }
        public int RecursoActividadId { get; set; }
        public int EstudianteId { get; set; }
        public int? InstructorId { get; set; } 
        public string UrlArchivoEntrega { get; set; } = string.Empty;
        public string? ComentarioEstudiante { get; set; }
        public DateTime FechaEntrega { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Pendiente";
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Calificacion { get; set; }
        public string? ObservacionInstructor { get; set; }
        public DateTime? FechaCalificacion { get; set; }
        public RecursoActividad RecursoActividad { get; set; } = null!;
        public Usuario Estudiante { get; set; } = null!; 
        public Artista? Instructor { get; set; }
    }
}