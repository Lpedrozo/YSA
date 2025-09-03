using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Artista
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        [StringLength(255)]
        public string NombreArtistico { get; set; }

        public string Biografia { get; set; }

        [StringLength(255)]
        public string EstiloPrincipal { get; set; }

        // Relaciones de navegación
        public virtual ICollection<Curso> Cursos { get; set; }
        public virtual ICollection<Producto> Productos { get; set; }
    }
}