using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ICompraService
    {
        Task<bool> ProcesarCompraProductoAsync(int productoId, int userId);
        Task<IEnumerable<int>> GetPurchasedProductIdsAsync(int userId);
        Task<bool> HasUserPurchasedProductAsync(int userId, int productoId);
        Task<Pago> RegistrarPagoAsync(int pedidoId, Pago pago);
        Task<Pedido> IniciarCompraProductoAsync(int productoId, int userId);
        Task<IEnumerable<int>> GetProductsInValidationIdsAsync(int userId); // <--- Nuevo método

    }
}