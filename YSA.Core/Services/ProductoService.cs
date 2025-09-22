using YSA.Core.Entities;
using YSA.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;

        public ProductoService(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<Producto> GetByIdAsync(int id)
        {
            return await _productoRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _productoRepository.GetAllAsync();
        }
        public async Task AddProductoAsync(Producto producto, IEnumerable<int> categoriaIds)
        {
            producto.ProductoCategorias = categoriaIds.Select(id => new ProductoCategoria
            {
                CategoriaId = id
            }).ToList();

            await _productoRepository.AddAsync(producto);
        }

        public async Task UpdateProductoAsync(Producto producto, IEnumerable<int> categoriaIds)
        {
            // Lógica para actualizar las categorías
            var productoExistente = await _productoRepository.GetByIdAsync(producto.Id);
            if (productoExistente != null)
            {
                // Eliminar las categorías existentes que ya no están seleccionadas
                var categoriasAEliminar = productoExistente.ProductoCategorias.Where(pc => !categoriaIds.Contains(pc.CategoriaId)).ToList();
                foreach (var pc in categoriasAEliminar)
                {
                    productoExistente.ProductoCategorias.Remove(pc);
                }

                // Agregar las nuevas categorías
                var categoriasAAgregar = categoriaIds.Where(id => !productoExistente.ProductoCategorias.Any(pc => pc.CategoriaId == id)).ToList();
                foreach (var id in categoriasAAgregar)
                {
                    productoExistente.ProductoCategorias.Add(new ProductoCategoria { ProductoId = productoExistente.Id, CategoriaId = id });
                }

                productoExistente.Titulo = producto.Titulo;
                productoExistente.DescripcionCorta = producto.DescripcionCorta;
                productoExistente.DescripcionLarga = producto.DescripcionLarga;
                productoExistente.Precio = producto.Precio;
                productoExistente.UrlImagen = producto.UrlImagen;
                productoExistente.UrlArchivoDigital = producto.UrlArchivoDigital;
                productoExistente.AutorId = producto.AutorId;
                productoExistente.TipoProducto = producto.TipoProducto;

                await _productoRepository.UpdateAsync(productoExistente);
            }
        }

        public async Task DeleteProductoAsync(int id)
        {
            await _productoRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Categoria>> GetCategoriasAsync()
        {
            return await _productoRepository.GetCategoriasAsync();
        }

        public async Task<IEnumerable<Artista>> GetAutoresAsync()
        {
            return await _productoRepository.GetAutoresAsync();
        }
    }
}