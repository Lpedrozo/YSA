using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Services
{
    public interface IProductoService
    {
        Task<Producto> GetByIdAsync(int id);
        Task<IEnumerable<Producto>> GetAllAsync();
        Task AddProductoAsync(Producto producto, IEnumerable<int> categoriaIds);
        Task UpdateProductoAsync(Producto producto, IEnumerable<int> categoriaIds);
        Task DeleteProductoAsync(int id);
        Task<IEnumerable<Categoria>> GetCategoriasAsync();
        Task<IEnumerable<Artista>> GetAutoresAsync();
    }
}