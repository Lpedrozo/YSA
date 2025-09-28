using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace YSA.Core.Services
{
    public class CompraService : ICompraService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IVentaItemRepository _ventaItemRepository;
        private readonly IEstudianteCursoRepository _estudianteCursoRepository;

        // Inyectamos todos los repositorios que necesitamos
        public CompraService(IProductoRepository productoRepository,
                             IPedidoRepository pedidoRepository,
                             IVentaItemRepository ventaItemRepository,
                             IEstudianteCursoRepository estudianteCursoRepository)
        {
            _productoRepository = productoRepository;
            _pedidoRepository = pedidoRepository;
            _ventaItemRepository = ventaItemRepository;
            _estudianteCursoRepository = estudianteCursoRepository;
        }

        public async Task<bool> ProcesarCompraProductoAsync(int productoId, int userId)
        {
            var producto = await _productoRepository.GetByIdAsync(productoId);
            if (producto == null)
            {
                return false;
            }

            // Crear el VentaItem (si no existe)
            var ventaItem = await _ventaItemRepository.GetByProductoIdAsync(productoId);
            if (ventaItem == null)
            {
                ventaItem = new VentaItem
                {
                    Tipo = producto.TipoProducto,
                    ProductoId = producto.Id,
                    Precio = producto.Precio
                };
                await _ventaItemRepository.AddAsync(ventaItem);
            }

            // Crear el Pedido
            var nuevoPedido = new Pedido
            {
                EstudianteId = userId,
                FechaPedido = DateTime.UtcNow,
                Estado = "Completado", // O "Pendiente" si manejas un flujo de pago
                Total = ventaItem.Precio,
                PedidoItems = new List<PedidoItem>
                {
                    new PedidoItem
                    {
                        VentaItemId = ventaItem.Id,
                        PrecioUnidad = ventaItem.Precio,
                        Cantidad = 1
                    }
                }
            };

            await _pedidoRepository.AddAsync(nuevoPedido);
            return true;
        }

        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productoId)
        {
            var pedidosCompletados = await _pedidoRepository.GetPedidosByUsuarioAndEstadoAsync(userId, "Completado");

            foreach (var pedido in pedidosCompletados)
            {
                var ventaItems = await _ventaItemRepository.GetItemsByPedidoIdAsync(pedido.Id);
                if (ventaItems.Any(vi => vi.ProductoId == productoId))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<IEnumerable<int>> GetPurchasedProductIdsAsync(int userId)
        {
            var pedidosCompletados = await _pedidoRepository.GetPedidosByUsuarioAndEstadoAsync(userId, "Completado");
            var purchasedProductIds = new HashSet<int>();

            foreach (var pedido in pedidosCompletados)
            {
                var ventaItems = await _ventaItemRepository.GetItemsByPedidoIdAsync(pedido.Id);
                foreach (var ventaItem in ventaItems)
                {
                    if (ventaItem.ProductoId.HasValue)
                    {
                        purchasedProductIds.Add(ventaItem.ProductoId.Value);
                    }
                }
            }

            return purchasedProductIds;
        }
        public async Task<Pedido> IniciarCompraProductoAsync(int productoId, int userId)
        {
            var producto = await _productoRepository.GetByIdAsync(productoId);
            if (producto == null)
            {
                return null;
            }

            var ventaItem = await _ventaItemRepository.GetByProductoIdAsync(productoId);
            if (ventaItem == null)
            {
                ventaItem = new VentaItem
                {
                    Tipo = producto.TipoProducto,
                    ProductoId = producto.Id,
                    Precio = producto.Precio
                };
                await _ventaItemRepository.AddAsync(ventaItem);
            }

            var nuevoPedido = new Pedido
            {
                EstudianteId = userId,
                FechaPedido = DateTime.UtcNow,
                Estado = "Pendiente", // Estado inicial
                Total = ventaItem.Precio,
                PedidoItems = new List<PedidoItem>
                {
                    new PedidoItem
                    {
                        VentaItemId = ventaItem.Id,
                        PrecioUnidad = ventaItem.Precio,
                        Cantidad = 1
                    }
                }
            };

            return await _pedidoRepository.AddAsync(nuevoPedido);
        }

        // Nuevo método para registrar el pago
        public async Task<Pago> RegistrarPagoAsync(int pedidoId, Pago pago)
        {
            var pedido = await _pedidoRepository.GetByIdAsync(pedidoId);
            if (pedido == null)
            {
                throw new ArgumentException("El pedido no existe.");
            }

            pago.PedidoId = pedidoId;
            pago.FechaPago = DateTime.UtcNow;

            pedido.Estado = "Validando";
            await _pedidoRepository.UpdateAsync(pedido);

            return await _pedidoRepository.AddPagoAsync(pago);
        }
        public async Task<IEnumerable<int>> GetProductsInValidationIdsAsync(int userId)
        {
            // Obtiene todos los pedidos del usuario que están en estado "Validando"
            var pedidosEnValidacion = await _pedidoRepository.GetPedidosByUsuarioAndEstadoAsync(userId, "Validando");
            var productosEnValidacionIds = new HashSet<int>();

            foreach (var pedido in pedidosEnValidacion)
            {
                var ventaItems = await _ventaItemRepository.GetItemsByPedidoIdAsync(pedido.Id);
                foreach (var ventaItem in ventaItems)
                {
                    if (ventaItem.ProductoId.HasValue)
                    {
                        productosEnValidacionIds.Add(ventaItem.ProductoId.Value);
                    }
                }
            }

            return productosEnValidacionIds;
        }
    }
}