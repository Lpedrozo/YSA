using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class InscripcionClase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClasePresencialId { get; set; }

        [ForeignKey("ClasePresencialId")]
        public virtual ClasePresencial ClasePresencial { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

        // Estado de asistencia: Pendiente, Confirmada, Ausente, Asistio
        [StringLength(50)]
        public string EstadoAsistencia { get; set; } = "Pendiente";

        // Fecha y hora en que el estudiante confirmó o marcó asistencia
        public DateTime? FechaConfirmacion { get; set; }

        // Comentario del estudiante (opcional)
        [StringLength(500)]
        public string? Comentario { get; set; }
    }
}