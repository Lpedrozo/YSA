using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YSA.Core.Entities;

namespace YSA.Core.Entities
{
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        public int EstudianteId { get; set; }
        public Usuario Estudiante { get; set; }

        public DateTime FechaPedido { get; set; } = DateTime.UtcNow;

        [Required]
        public string Estado { get; set; } // "Pendiente", "Validando", "Completado", "Cancelado"

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Total { get; set; }

        // Relaciones de navegación
        public virtual ICollection<PedidoItem> PedidoItems { get; set; }
        public virtual Pago Pago { get; set; }
    }
}