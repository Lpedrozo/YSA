using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Core.Services
{
    public interface IPedidoService
    {
        Task<Pedido> CrearPedidoAsync(int estudianteId, List<int> ventaItemIds); // Cambia de productoIds a ventaItemIds
        Task<Pedido> ObtenerPedidoPorIdAsync(int pedidoId);
        Task<Pedido> ObtenerPedidoConItemsYVentaItemsAsync(int pedidoId); // Nuevo método
        Task<bool> ActualizarEstadoPedidoAsync(int pedidoId, string nuevoEstado);
        Task<Pago> RegistrarPagoAsync(Pago pago);
        Task<IEnumerable<Pedido>> ObtenerPedidosPorEstadoAsync(string estado);
        Task AprobarPedidoYOtorgarAccesoAsync(int pedidoId);
        Task<bool> TienePedidoPendientePorCursoAsync(int estudianteId, int cursoId); // Nuevo método

    }
}