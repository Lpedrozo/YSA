namespace YSA.Web.Models.ViewModels
{
    public class ConfirmacionPagoViewModel
    {
        public int PedidoId { get; set; }
        public decimal Total { get; set; }
        public List<ArticuloPedidoViewModel> Articulos { get; set; }
        public decimal? TasaBCV { get; set; }

    }

    // Usado para mostrar cada ítem en el resumen del pedido
    public class ArticuloPedidoViewModel
    {
        // Cambiamos el nombre para que sea más genérico
        public string TituloItem { get; set; }
        public decimal Precio { get; set; }
    }
    public class PedidoPendienteViewModel
    {
        public int PedidoId { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string UrlComprobante { get; set; }
        // Opcional: El estado del pedido original puede ser útil en la vista.
        public string Estado { get; set; }
    }

    public class DashboardPedidosViewModel
    {
        public List<PedidoPendienteViewModel> PedidosValidando { get; set; } = new List<PedidoPendienteViewModel>();
        public List<PedidoPendienteViewModel> PedidosCompletados { get; set; } = new List<PedidoPendienteViewModel>();
        public List<PedidoPendienteViewModel> PedidosCancelados { get; set; } = new List<PedidoPendienteViewModel>();
        public List<PedidoPendienteViewModel> PedidosPendientes { get; set; } = new List<PedidoPendienteViewModel>(); // Sin pago adjunto

        // Propiedad para saber qué pestaña está activa inicialmente
        public string EstadoActivo { get; set; } = "Validando";
    }
}