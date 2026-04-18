using System.Collections.Generic;
using System.Threading.Tasks;
using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface ICompraService
    {
        // Productos
        Task<bool> ProcesarCompraProductoAsync(int productoId, int userId);
        Task<IEnumerable<int>> GetPurchasedProductIdsAsync(int userId);
        Task<bool> HasUserPurchasedProductAsync(int userId, int productoId);
        Task<Pago> RegistrarPagoAsync(int pedidoId, Pago pago);
        Task<Pedido> IniciarCompraProductoAsync(int productoId, int userId);
        Task<IEnumerable<int>> GetProductsInValidationIdsAsync(int userId);

        // Paquetes
        Task<Pedido> IniciarCompraPaqueteAsync(int paqueteId, int userId);
        Task<bool> HasUserPurchasedPackageAsync(int userId, int paqueteId);
        Task<IEnumerable<int>> GetPurchasedPackageIdsAsync(int userId);
        Task<IEnumerable<int>> GetPackagesInValidationIdsAsync(int userId);
        Task RegistrarPagoPaqueteAsync(int pedidoId, Pago pago);
        Task<List<Paquete>> GetPurchasedPackagesWithDetailsAsync(int userId);

        // Cursos (para cuando se aprueba un paquete)
        Task OtorgarAccesoPaqueteAsync(int pedidoId);
    }
}