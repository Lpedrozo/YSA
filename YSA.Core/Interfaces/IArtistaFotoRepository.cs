using YSA.Core.Entities;

namespace YSA.Core.Interfaces
{
    public interface IArtistaFotoRepository
    {
        Task<ArtistaFoto> GetByIdAsync(int id);
        Task<List<ArtistaFoto>> GetByArtistaIdAsync(int artistaId);
        Task AddAsync(ArtistaFoto foto);
        Task DeleteAsync(ArtistaFoto foto);
        Task SaveChangesAsync();
    }
}