using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public interface IVentaItemService
    {
        Task<VentaItem> ObtenerVentaItemPorCursoIdAsync(int cursoId);
        Task<VentaItem> ObtenerVentaItemPorProductoIdAsync(int productoId);
        Task<IEnumerable<VentaItem>> ObtenerVentaItemsPorIdsAsync(List<int> ids);
        Task<VentaItem> CrearVentaItemAsync(string tipo, int? cursoId = null, int? productoId = null, decimal? precio = null);
        Task ActualizarVentaItemAsync(VentaItem ventaItem);
        Task<IEnumerable<VentaItem>> ObtenerItemsPorPedidoIdAsync(int pedidoId);

    }
}