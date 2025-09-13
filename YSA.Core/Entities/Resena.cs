using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Resena
    {
        [Required]
        public int EstudianteId { get; set; }
        [ForeignKey("EstudianteId")]
        public Usuario Estudiante { get; set; }

        [Required]
        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe ser entre 1 y 5.")]
        public int Calificacion { get; set; }

        public string Comentario { get; set; }

        public DateTime FechaResena { get; set; }
    }
}