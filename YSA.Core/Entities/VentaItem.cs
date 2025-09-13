using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class VentaItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } // "Curso", "Producto"

        public int? CursoId { get; set; }
        public Curso Curso { get; set; }

        public int? ProductoId { get; set; }
        public Producto Producto { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Precio { get; set; }
        public ICollection<PedidoItem> PedidoItems { get; set; }

    }
}