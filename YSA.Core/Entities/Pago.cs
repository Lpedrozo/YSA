using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Pago
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; }

        [StringLength(255)]
        public string ReferenciaPago { get; set; }

        [StringLength(255)]
        public string UrlComprobante { get; set; }

        public DateTime FechaPago { get; set; } = DateTime.UtcNow;

        public int? ValidadorId { get; set; }
        public Usuario Validador { get; set; }
    }
}