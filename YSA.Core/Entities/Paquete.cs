using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class Paquete
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

        public bool EsDestacado { get; set; }

        public bool EsRecomendado { get; set; }

        // Relaciones
        public virtual ICollection<PaqueteCurso> PaqueteCursos { get; set; } = new List<PaqueteCurso>();
        public virtual ICollection<PaqueteProducto> PaqueteProductos { get; set; } = new List<PaqueteProducto>();

        // Precio total del paquete (suma de todos los cursos y productos)
        [NotMapped]
        public decimal PrecioTotalItems => (PaqueteCursos?.Sum(pc => pc.Curso?.Precio ?? 0) ?? 0) +
                                            (PaqueteProductos?.Sum(pp => pp.Producto?.Precio ?? 0) ?? 0);

        // Ahorro al comprar el paquete
        [NotMapped]
        public decimal Ahorro => PrecioTotalItems - Precio;
    }
}