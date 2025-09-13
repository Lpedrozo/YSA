using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IVentaItemRepository
    {
        Task<VentaItem> GetByIdAsync(int id);
        Task<VentaItem> GetByCursoIdAsync(int cursoId);
        Task<VentaItem> GetByProductoIdAsync(int productoId);
        Task<IEnumerable<VentaItem>> GetByIdsAsync(IEnumerable<int> ids);
        Task<VentaItem> AddAsync(VentaItem ventaItem);
        Task UpdateAsync(VentaItem ventaItem);
        Task<IEnumerable<VentaItem>> GetItemsByPedidoIdAsync(int pedidoId);

    }
}