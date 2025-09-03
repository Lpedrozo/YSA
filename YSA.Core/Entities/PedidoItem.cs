using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class PedidoItem
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public int VentaItemId { get; set; } // Cambia a VentaItem
        public VentaItem VentaItem { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioUnidad { get; set; }

        [Required]
        public int Cantidad { get; set; }
    }
}