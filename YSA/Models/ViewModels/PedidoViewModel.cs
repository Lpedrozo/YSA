namespace YSA.Web.Models.ViewModels
{
    public class ConfirmacionPagoViewModel
    {
        public int PedidoId { get; set; }
        public decimal Total { get; set; }
        public List<ArticuloPedidoViewModel> Articulos { get; set; }
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
    }
}