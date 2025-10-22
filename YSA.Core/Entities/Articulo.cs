using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class Articulo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Titulo { get; set; }

        public string Resumen { get; set; }

        [Required]
        public string ContenidoTexto { get; set; }

        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Categoria { get; set; }

        [StringLength(20)]
        public string Estado { get; set; } = "Borrador";

        // --- Detalles de la Persona Destacada/Entrevistada (Autocontenidos) ---
        [Required]
        [StringLength(100)]
        public string NombrePersonaDestacada { get; set; }

        public string BiografiaCortaDestacado { get; set; }

        [StringLength(255)]
        public string UrlFotoDestacado { get; set; }

        // --- Imagen Principal del Artículo ---
        [StringLength(255)]
        public string UrlImagenPrincipal { get; set; }

        // --- Relación de navegación: Fotos de contenido del artículo ---
        public virtual ICollection<ArticuloFoto> Fotos { get; set; } = new List<ArticuloFoto>();
    }
}
