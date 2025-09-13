using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Anuncio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CursoId { get; set; }
        [ForeignKey("CursoId")]
        public Curso Curso { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        public string Contenido { get; set; }

        public DateTime FechaPublicacion { get; set; }
    }
}