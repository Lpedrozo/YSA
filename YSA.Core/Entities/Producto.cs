using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        [StringLength(255)]
        public string DescripcionCorta { get; set; }

        public string DescripcionLarga { get; set; }

        [Required]
        [StringLength(50)]
        public string TipoProducto { get; set; } // "pdf" o "revista"

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }

        [StringLength(255)]
        public string UrlImagen { get; set; }

        [StringLength(255)]
        public string UrlArchivoDigital { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        public int? AutorId { get; set; }
        public Artista Autor { get; set; }

        // Relaciones de navegación
        public virtual ICollection<ProductoCategoria> ProductoCategorias { get; set; }
    }
}