using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        public string? Titulo { get; set; }

        public string? Resumen { get; set; }

        public string? ContenidoTexto { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string? Categoria { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // --- Campos simplificados (opcionales, solo si se necesitan) ---
        [StringLength(255)]
        public string? UrlImagenPortada { get; set; }  // Portada del artículo

        [StringLength(255)]
        public string? UrlFotoDestacado { get; set; }  // Opcional

        // --- Relación de navegación: Fotos de galería del artículo ---
        public virtual ICollection<ArticuloFoto> Fotos { get; set; } = new List<ArticuloFoto>();
    }
}