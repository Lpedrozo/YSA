using YSA.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YSA.Core.Interfaces
{
    public interface IProductoRepository
    {
        Task<Producto> GetByIdAsync(int id);
        Task<IEnumerable<Producto>> GetAllAsync();
        Task<Producto> AddAsync(Producto producto);
        Task<bool> UpdateAsync(Producto producto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Categoria>> GetCategoriasAsync();
        Task<IEnumerable<Artista>> GetAutoresAsync();
    }
}