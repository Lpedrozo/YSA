using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YSA.Core.Entities
{
    public class ArticuloFoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ArticuloId { get; set; }

        [Required]
        [StringLength(255)]
        public string UrlFoto { get; set; }

        [StringLength(255)]
        public string Descripcion { get; set; }

        public int Orden { get; set; } = 0;

        // --- Relación de navegación ---
        [ForeignKey(nameof(ArticuloId))]
        public virtual Articulo Articulo { get; set; }
    }
}
