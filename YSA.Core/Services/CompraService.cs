using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YSA.Core.Entities;
using YSA.Core.Interfaces;

namespace YSA.Core.Services
{
    public class CompraService : ICompraService
    {
        private readonly IVentaItemService _ventaItemService;
        private readonly IPedidoService _pedidoService;
        private readonly IProductoService _productoService;
        private readonly IPaqueteService _paqueteService;
        private readonly ICursoService _cursoService;
        private readonly IEstudianteCursoService _estudianteCursoService;

        public CompraService(
            IVentaItemService ventaItemService,
            IPedidoService pedidoService,
            IProductoService productoService,
            IPaqueteService paqueteService,
            ICursoService cursoService,
            IEstudianteCursoService estudianteCursoService)
        {
            _ventaItemService = ventaItemService;
            _pedidoService = pedidoService;
            _productoService = productoService;
            _paqueteService = paqueteService;
            _cursoService = cursoService;
            _estudianteCursoService = estudianteCursoService;
        }

        // ==================== PRODUCTOS ====================

        public async Task<Pedido> IniciarCompraProductoAsync(int productoId, int userId)
        {
            var producto = await _productoService.GetByIdAsync(productoId);
            if (producto == null) return null;

            var ventaItem = await _ventaItemService.ObtenerVentaItemPorProductoIdAsync(productoId);
            if (ventaItem == null)
            {
                await _ventaItemService.CrearVentaItemAsync("Producto", null, productoId, producto.Precio);
                ventaItem = await _ventaItemService.ObtenerVentaItemPorProductoIdAsync(productoId);
            }

            var pedido = await _pedidoService.CrearPedidoAsync(userId, new List<int> { ventaItem.Id });
            return pedido;
        }

        public async Task<Pago> RegistrarPagoAsync(int pedidoId, Pago pago)
        {
            await _pedidoService.RegistrarPagoAsync(pago);
            await _pedidoService.ActualizarEstadoPedidoAsync(pedidoId, "Validando");
            return pago;
        }

        public async Task<bool> ProcesarCompraProductoAsync(int productoId, int userId)
        {
            var pedido = await IniciarCompraProductoAsync(productoId, userId);
            return pedido != null;
        }

        public async Task<IEnumerable<int>> GetPurchasedProductIdsAsync(int userId)
        {
            // 1. Productos comprados individualmente
            var pedidosCompletados = await _pedidoService.ObtenerPedidosCompletadosPorUsuarioAsync(userId);
            var productosIds = new List<int>();

            foreach (var pedido in pedidosCompletados)
            {
                foreach (var item in pedido.PedidoItems)
                {
                    // Productos individuales
                    if (item.VentaItem.ProductoId.HasValue)
                    {
                        productosIds.Add(item.VentaItem.ProductoId.Value);
                    }

                    // Productos dentro de paquetes
                    if (item.VentaItem.PaqueteId.HasValue)
                    {
                        var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(item.VentaItem.PaqueteId.Value);
                        if (paquete?.PaqueteProductos != null)
                        {
                            foreach (var pp in paquete.PaqueteProductos)
                            {
                                if (pp.ProductoId > 0)
                                {
                                    productosIds.Add(pp.ProductoId);
                                }
                            }
                        }
                    }
                }
            }

            return productosIds.Distinct();
        }

        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productoId)
        {
            var productosComprados = await GetPurchasedProductIdsAsync(userId);
            return productosComprados.Contains(productoId);
        }

        public async Task<IEnumerable<int>> GetProductsInValidationIdsAsync(int userId)
        {
            var pedidosValidando = await _pedidoService.ObtenerPedidosPorEstadoYUsuarioAsync("Validando", userId);
            var productosIds = new List<int>();

            foreach (var pedido in pedidosValidando)
            {
                foreach (var item in pedido.PedidoItems)
                {
                    // Productos individuales
                    if (item.VentaItem.ProductoId.HasValue)
                    {
                        productosIds.Add(item.VentaItem.ProductoId.Value);
                    }

                    // Productos dentro de paquetes
                    if (item.VentaItem.PaqueteId.HasValue)
                    {
                        var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(item.VentaItem.PaqueteId.Value);
                        if (paquete?.PaqueteProductos != null)
                        {
                            foreach (var pp in paquete.PaqueteProductos)
                            {
                                if (pp.ProductoId > 0)
                                {
                                    productosIds.Add(pp.ProductoId);
                                }
                            }
                        }
                    }
                }
            }

            return productosIds.Distinct();
        }

        // ==================== PAQUETES ====================

        public async Task<Pedido> IniciarCompraPaqueteAsync(int paqueteId, int userId)
        {
            var paquete = await _paqueteService.ObtenerPorIdAsync(paqueteId);
            if (paquete == null) return null;

            var ventaItem = await _ventaItemService.ObtenerVentaItemPorPaqueteIdAsync(paqueteId);
            if (ventaItem == null)
            {
                await _ventaItemService.CrearVentaItemAsync("Paquete", null, null, paquete.Precio, paqueteId);
                ventaItem = await _ventaItemService.ObtenerVentaItemPorPaqueteIdAsync(paqueteId);
            }

            var pedido = await _pedidoService.CrearPedidoAsync(userId, new List<int> { ventaItem.Id });
            return pedido;
        }

        public async Task<bool> HasUserPurchasedPackageAsync(int userId, int paqueteId)
        {
            var paquetesComprados = await GetPurchasedPackageIdsAsync(userId);
            return paquetesComprados.Contains(paqueteId);
        }

        public async Task<IEnumerable<int>> GetPurchasedPackageIdsAsync(int userId)
        {
            var pedidosCompletados = await _pedidoService.ObtenerPedidosCompletadosPorUsuarioAsync(userId);
            var paquetesIds = new List<int>();

            foreach (var pedido in pedidosCompletados)
            {
                foreach (var item in pedido.PedidoItems)
                {
                    if (item.VentaItem.PaqueteId.HasValue)
                    {
                        paquetesIds.Add(item.VentaItem.PaqueteId.Value);
                    }
                }
            }

            return paquetesIds.Distinct();
        }

        public async Task<IEnumerable<int>> GetPackagesInValidationIdsAsync(int userId)
        {
            var pedidosValidando = await _pedidoService.ObtenerPedidosPorEstadoYUsuarioAsync("Validando", userId);
            var paquetesIds = new List<int>();

            foreach (var pedido in pedidosValidando)
            {
                foreach (var item in pedido.PedidoItems)
                {
                    if (item.VentaItem.PaqueteId.HasValue)
                    {
                        paquetesIds.Add(item.VentaItem.PaqueteId.Value);
                    }
                }
            }

            return paquetesIds.Distinct();
        }

        public async Task RegistrarPagoPaqueteAsync(int pedidoId, Pago pago)
        {
            await _pedidoService.RegistrarPagoAsync(pago);
            await _pedidoService.ActualizarEstadoPedidoAsync(pedidoId, "Validando");
        }

        public async Task<List<Paquete>> GetPurchasedPackagesWithDetailsAsync(int userId)
        {
            var paquetesIds = await GetPurchasedPackageIdsAsync(userId);
            var paquetes = new List<Paquete>();

            foreach (var id in paquetesIds)
            {
                var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(id);
                if (paquete != null)
                {
                    paquetes.Add(paquete);
                }
            }

            return paquetes;
        }

        public async Task OtorgarAccesoPaqueteAsync(int pedidoId)
        {
            var pedido = await _pedidoService.ObtenerPedidoConItemsYVentaItemsAsync(pedidoId);
            if (pedido == null) return;

            foreach (var item in pedido.PedidoItems)
            {
                if (item.VentaItem.PaqueteId.HasValue)
                {
                    var paquete = await _paqueteService.ObtenerPorIdConDetallesAsync(item.VentaItem.PaqueteId.Value);
                    if (paquete != null)
                    {
                        // Otorgar acceso a cada curso del paquete
                        if (paquete.PaqueteCursos != null)
                        {
                            foreach (var pc in paquete.PaqueteCursos)
                            {
                                var yaTieneAcceso = await _estudianteCursoService.TieneAccesoAlCursoAsync(pedido.EstudianteId, pc.CursoId);
                                if (!yaTieneAcceso)
                                {
                                    await _estudianteCursoService.OtorgarAccesoAsync(pedido.EstudianteId, pc.CursoId);
                                }
                            }
                        }

                        // Los productos digitales no requieren acceso, solo la descarga
                        // Los productos ya están disponibles para descarga a través del enlace
                    }
                }
            }
        }
    }
}