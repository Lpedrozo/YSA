using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IArticuloRepository
    {
        Task<Articulo> GetByIdAsync(int id);
        Task<IEnumerable<Articulo>> GetAllAsync();
        Task AddAsync(Articulo articulo);
        Task UpdateAsync(Articulo articulo);
        Task DeleteAsync(int id);
        Task<ArticuloFoto> GetFotoByIdAsync(int id);
        Task DeleteFotoAsync(int id);
    }
}