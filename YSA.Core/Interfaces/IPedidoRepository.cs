using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IPedidoRepository
    {
        Task<Pedido> GetByIdAsync(int id);
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task<Pedido> AddAsync(Pedido pedido);
        Task<bool> UpdateAsync(Pedido pedido);
        Task<bool> DeleteAsync(int id);
        Task<Pedido> GetPedidoWithItemsAndVentaItemsAsync(int id);
        Task<PedidoItem> AddPedidoItemAsync(PedidoItem item);
        Task<Pago> AddPagoAsync(Pago pago);
        Task<IEnumerable<Pedido>> GetPedidosByEstadoAsync(string estado);
        Task<bool> ExistePedidoEnEstadoParaCursoAsync(int estudianteId, int cursoId, string estado); // Nuevo método
        Task<IEnumerable<Pedido>> GetPedidosByUsuarioAndEstadoAsync(int estudianteId, string estado);
        Task<IEnumerable<VentaItem>> GetItemsByPedidoIdAsync(int pedidoId);
        Task<Pago> GetPagoWithPedido(int id);
        Task<int> GetPedidosPendientesAsync();
        Task<Pedido> GetByIdWithDetailsAsync(int id);
        Task<Pedido> ObtenerPedidoActivoPorCursoAsync(int estudianteId, int cursoId, string[] estadosActivos);
        Task<int?> ObtenerPedidoPendienteIdPorCursoAsync(int estudianteId, int cursoId);
        Task<List<Pedido>> ObtenerPedidosPorUsuarioAsync(int usuarioId); // ← NUEVO MÉTODO

    }
}