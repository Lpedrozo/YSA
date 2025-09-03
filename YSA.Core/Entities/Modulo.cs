using System.ComponentModel.DataAnnotations;
using static System.Collections.Specialized.BitVector32;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Modulo
    {
        [Key]
        public int Id { get; set; }

        public int CursoId { get; set; }
        public Curso Curso { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [Required]
        public int Orden { get; set; }

        // Relaciones de navegación
        public virtual ICollection<Leccion> Lecciones { get; set; }
    }
}