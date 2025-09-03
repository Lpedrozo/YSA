using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace YSA.Core.Services
{
    public class VentaItemService : IVentaItemService
    {
        private readonly IVentaItemRepository _ventaItemRepository;
        private readonly ICursoRepository _cursoRepository;
        private readonly IProductoRepository _productoRepository;

        public VentaItemService(IVentaItemRepository ventaItemRepository, ICursoRepository cursoRepository, IProductoRepository productoRepository)
        {
            _ventaItemRepository = ventaItemRepository;
            _cursoRepository = cursoRepository;
            _productoRepository = productoRepository;
        }

        public async Task<VentaItem> ObtenerVentaItemPorCursoIdAsync(int cursoId)
        {
            var ventaItem = await _ventaItemRepository.GetByCursoIdAsync(cursoId);
            if (ventaItem == null)
            {
                var curso = await _cursoRepository.GetByIdAsync(cursoId);
                if (curso != null)
                {
                    // Si el VentaItem no existe, lo creamos
                    ventaItem = new VentaItem
                    {
                        Tipo = "Curso",
                        CursoId = curso.Id,
                        Precio = curso.Precio // Asumiendo que el curso tiene un precio
                    };
                    await _ventaItemRepository.AddAsync(ventaItem);
                }
            }
            return ventaItem;
        }

        public async Task<VentaItem> ObtenerVentaItemPorProductoIdAsync(int productoId)
        {
            var ventaItem = await _ventaItemRepository.GetByProductoIdAsync(productoId);
            if (ventaItem == null)
            {
                var producto = await _productoRepository.GetByIdAsync(productoId);
                if (producto != null)
                {
                    // Si el VentaItem no existe, lo creamos
                    ventaItem = new VentaItem
                    {
                        Tipo = producto.TipoProducto,
                        ProductoId = producto.Id,
                        Precio = producto.Precio
                    };
                    await _ventaItemRepository.AddAsync(ventaItem);
                }
            }
            return ventaItem;
        }
        
        public async Task<IEnumerable<VentaItem>> ObtenerVentaItemsPorIdsAsync(List<int> ids)
        {
            return await _ventaItemRepository.GetByIdsAsync(ids);
        }

        public async Task<VentaItem> CrearVentaItemAsync(string tipo, int? cursoId = null, int? productoId = null, decimal? precio = null)
        {
            // Lógica para crear un VentaItem de forma manual si se necesita, aunque los métodos de arriba ya lo hacen
            return new VentaItem
            {
                Tipo = tipo,
                CursoId = cursoId,
                ProductoId = productoId,
                Precio = precio ?? 0
            };
        }
        public async Task ActualizarVentaItemAsync(VentaItem ventaItem)
        {
            await _ventaItemRepository.UpdateAsync(ventaItem);
        }
    }
}