using System.ComponentModel.DataAnnotations;

namespace YSA.Web.Models.ViewModels
{
    public class PagoViewModel
    {
        [Required]
        public int PedidoId { get; set; }

        public decimal Total { get; set; }

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; }

        [StringLength(255)]
        public string ReferenciaPago { get; set; }

        public IFormFile ComprobanteArchivo { get; set; }
    }
}