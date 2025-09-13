using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class PreguntaRespuesta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }

        [Required]
        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public Usuario Estudiante { get; set; }

        [Required]
        public string Pregunta { get; set; }

        public DateTime FechaPregunta { get; set; }

        public string Respuesta { get; set; }

        public int? InstructorId { get; set; }
        [ForeignKey("InstructorId")]
        public Artista Instructor { get; set; }

        public DateTime? FechaRespuesta { get; set; }
    }
}