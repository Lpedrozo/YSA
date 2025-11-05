using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public interface INotificacionService
    {
        Task CrearNotificacionPagoConfirmadoAsync(int usuarioId, int pedidoId, decimal monto);
        Task CrearNotificacionPagoPendienteAsync(int usuarioId, int pedidoId, decimal monto);
        Task CrearNotificacionCursoCompradoAsync(int usuarioId, int cursoId, string cursoTitulo, int pedidoId); 
        Task CrearNotificacionProductoCompradoAsync(int usuarioId, int productoId, string productoTitulo);
        Task<List<Notificacion>> ObtenerNotificacionesUsuarioAsync(int usuarioId);
        Task<List<Notificacion>> ObtenerNotificacionesNoLeidasAsync(int usuarioId);
        Task MarcarComoLeidaAsync(int notificacionId, int usuarioId);
        Task MarcarTodasComoLeidasAsync(int usuarioId);
        Task<int> ObtenerCantidadNoLeidasAsync(int usuarioId);
        Task<bool> EliminarNotificacionAsync(int notificacionId, int usuarioId);
        Task CrearNotificacionNuevoPedidoAsync(int pedidoId, decimal monto, string tipoCompra);
        Task<List<int>> ObtenerIdsAdministradoresAsync();
        Task CrearNotificacionPedidoAprobadoAsync(int usuarioId, int pedidoId, string tipoCompra, string itemTitulo);
    }
}