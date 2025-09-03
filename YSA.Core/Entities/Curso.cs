using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Curso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [StringLength(255)]
        public string DescripcionCorta { get; set; }

        public string DescripcionLarga { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [StringLength(255)]
        public string UrlImagen { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        public int? InstructorId { get; set; }
        public Artista Instructor { get; set; }

        // Relaciones de navegación
        public virtual ICollection<Modulo> Modulos { get; set; }
        public virtual ICollection<CursoCategoria> CursoCategorias { get; set; }
    }
}